using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Update;
using Codelyzer.Analysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CTA.FeatureDetection;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.ProjectType.Extensions;
using System.IO;
using CTA.Rules.Metrics;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Reflection;

namespace CTA.Rules.PortCore
{
    /// <summary>
    /// Ports a solution
    /// </summary>
    public class SolutionPort
    {
        private SolutionRewriter _solutionRewriter;
        private string _solutionPath;
        private PortSolutionResult _portSolutionResult;
        private MetricsContext _context;

        /// <summary>
        /// Initializes a new instance of Solution Port, analyzing the solution path using the provided config.
        /// WARNING: This constructor will rebuild and reanalyze the solution, which will have a performance impact. If you 
        /// have an already analyzed solution, use another constructor
        /// </summary>
        /// <param name="solutionFilePath">Path to solution file</param>
        /// <param name="solutionConfiguration">Configuration for each project in solution to be built</param>
        public SolutionPort(string solutionFilePath, List<PortCoreConfiguration> solutionConfiguration, ILogger logger = null)
        {
            if (logger != null)
            {
                LogHelper.Logger = logger;
            }
            _portSolutionResult = new PortSolutionResult(solutionFilePath);
            _solutionPath = solutionFilePath;
            AnalyzerConfiguration analyzerConfiguration = new AnalyzerConfiguration(LanguageOptions.CSharp);
            analyzerConfiguration.MetaDataSettings = new MetaDataSettings()
            {
                Annotations = true,
                DeclarationNodes = true,
                MethodInvocations = true,
                ReferenceData = true,
                LoadBuildData = true,
                InterfaceDeclarations = true
            };

            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(analyzerConfiguration, LogHelper.Logger);
            var analyzerResults = analyzer.AnalyzeSolution(solutionFilePath).Result;

            _context = new MetricsContext(solutionFilePath, analyzerResults);
            InitSolutionRewriter(analyzerResults, solutionConfiguration);
        }

        public SolutionPort(string solutionFilePath,  List<AnalyzerResult> analyzerResults, List<PortCoreConfiguration> solutionConfiguration, ILogger logger = null)
        {
            if (logger != null)
            {
                LogHelper.Logger = logger;
            }
            _portSolutionResult = new PortSolutionResult(solutionFilePath);
            _solutionPath = solutionFilePath;
            _context = new MetricsContext(solutionFilePath, analyzerResults);
            InitSolutionRewriter(analyzerResults, solutionConfiguration);
        }

        private void InitSolutionRewriter(List<AnalyzerResult> analyzerResults, List<PortCoreConfiguration> solutionConfiguration)
        {
            CheckResources();
            InitRules(solutionConfiguration, analyzerResults);
            _solutionRewriter = new SolutionRewriter(analyzerResults, solutionConfiguration.ToList<ProjectConfiguration>());
        }

        private void DownloadRecommendationFiles(List<AnalyzerResult> analyzerResults)
        {
            var allReferences = analyzerResults
                .SelectMany(a => a.ProjectResult?.SourceFileResults?
                    .SelectMany(s => s.References))?
                .Select(r => r.Namespace).Distinct().ToList();

            allReferences.Add(Constants.ProjectRecommendationFile);

            _portSolutionResult.References = allReferences.ToHashSet<string>();
            
            using (var httpClient = new HttpClient())
            {
                ConcurrentBag<string> matchedFiles = new ConcurrentBag<string>();

                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
                Parallel.ForEach(allReferences, parallelOptions, recommendationNamespace =>
                {
                    if (!string.IsNullOrEmpty(recommendationNamespace))
                    {
                        try
                        {
                            var fileName = string.Concat(recommendationNamespace.ToLower(), ".json");
                            var fullFileName = Path.Combine(Constants.RulesDefaultPath, fileName);
                            //Download only if it's not available
                            if (!File.Exists(fullFileName))
                            {
                                var fileContents = httpClient.GetStringAsync(string.Concat(Constants.S3RecommendationsBucketUrl, "/", fileName)).Result;
                                File.WriteAllText(fullFileName, fileContents);
                            }
                            matchedFiles.Add(fileName);
                        }
                        catch (Exception ex)
                        {
                            //We are checking which files have a recommendation, some of them won't
                        }
                    }
                });

                _portSolutionResult.DownloadedFiles = matchedFiles.ToHashSet<string>();
                LogHelper.LogInformation("Found recommendations for the below:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, matchedFiles.Distinct()));
            }
        }

        private void InitRules(List<PortCoreConfiguration> solutionConfiguration, List<AnalyzerResult> analyzerResults)
        {
            var projectTypeFeatureDetector = new FeatureDetector();
            var projectTypeFeatureResults = projectTypeFeatureDetector.DetectFeaturesInProjects(analyzerResults);

            foreach (var projectConfiguration in solutionConfiguration)
            {
                var projectTypeFeatureResult = projectTypeFeatureResults[projectConfiguration.ProjectPath];
                projectConfiguration.ProjectType = GetProjectType(projectTypeFeatureResult);
                if (projectConfiguration.UseDefaultRules)
                {
                    DownloadRecommendationFiles(analyzerResults);
                    projectConfiguration.RulesPath = Constants.RulesDefaultPath;
                }
            }
        }
        /// <summary>
        /// Initializes the Solution Port
        /// </summary>
        public ConcurrentDictionary<string, ProjectActions> AnalysisRun()
        {
            return _solutionRewriter.AnalysisRun();
        }

        /// <summary>
        /// Runs the Solution Port after creating an analysis
        /// </summary>
        public PortSolutionResult Run()
        {
            _portSolutionResult.AddSolutionResult(_solutionRewriter.Run());
            if (!string.IsNullOrEmpty(_solutionPath))
            {
                PortSolutionResultReportGenerator reportGenerator = new PortSolutionResultReportGenerator(_context, _portSolutionResult);
                reportGenerator.GenerateAndExportReports();

                LogHelper.LogInformation("Generating Post-Build Report");
                LogHelper.LogError(Constants.MetricsTag, reportGenerator.PortSolutionResultJsonReport);
            }
            return _portSolutionResult;
        }

        private void CheckResources()
        {
            var executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var zipFile = Path.Combine(executingPath, "resources.zip");
                    var fileContents = httpClient.GetByteArrayAsync(string.Concat(Constants.S3CTAFiles)).Result;
                    File.WriteAllBytes(zipFile, fileContents);
                    ZipFile.ExtractToDirectory(zipFile, executingPath, true);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError("Unable to download resources");
                }
            }
        }

        private ProjectType GetProjectType(FeatureDetectionResult projectTypeFeatureResult)
        {
            if (projectTypeFeatureResult.IsMvcProject())
            {
                return ProjectType.Mvc;
            }
            else if (projectTypeFeatureResult.IsWebApiProject())
            {
                return ProjectType.WebApi;
            }
            else if (projectTypeFeatureResult.IsWebClassLibrary())
            {
                return ProjectType.WebClassLibrary;
            }
            return ProjectType.ClassLibrary;
        }
    }
}
