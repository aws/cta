using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.ProjectType.Extensions;
using CTA.Rules.Config;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using CTA.Rules.Update;
using Microsoft.Extensions.Logging;

namespace CTA.Rules.PortCore
{
    /// <summary>
    /// Ports a solution
    /// </summary>
    public class SolutionPort
    {
        private SolutionRewriter _solutionRewriter;
        private readonly string _solutionPath;
        private readonly PortSolutionResult _portSolutionResult;
        private readonly MetricsContext _context;
        private Dictionary<string, FeatureDetectionResult> _projectTypeFeatureResults;
        private readonly IDEProjectResult _projectResult;
        

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
            AnalyzerConfiguration analyzerConfiguration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                MetaDataSettings = new MetaDataSettings()
                {
                    Annotations = true,
                    DeclarationNodes = true,
                    MethodInvocations = true,
                    ReferenceData = true,
                    LoadBuildData = true,
                    InterfaceDeclarations = true,
                    MemberAccess = true,
                    ElementAccess = true
                }
            };

            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(analyzerConfiguration, LogHelper.Logger);

            List<AnalyzerResult> analyzerResults = null;
            //We are building using references
            if (solutionConfiguration.Any(p => p.MetaReferences?.Any() == true))
            {
                var currentReferences = solutionConfiguration.ToDictionary(s => s.ProjectPath, s => s.MetaReferences);
                var frameworkReferences = solutionConfiguration.ToDictionary(s => s.ProjectPath, s => s.FrameworkMetaReferences);
                analyzerResults = analyzer.AnalyzeSolution(solutionFilePath, frameworkReferences, currentReferences).Result;
            }
            else
            {
                analyzerResults = analyzer.AnalyzeSolution(solutionFilePath).Result;
            }


            _context = new MetricsContext(solutionFilePath, analyzerResults);
            InitSolutionRewriter(analyzerResults, solutionConfiguration);
        }

        public SolutionPort(string solutionFilePath, List<AnalyzerResult> analyzerResults, List<PortCoreConfiguration> solutionConfiguration, ILogger logger = null)
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

        public SolutionPort(string solutionFilePath, IDEProjectResult projectResult, List<PortCoreConfiguration> solutionConfiguration)
        {
            _solutionPath = solutionFilePath;
            _projectResult = projectResult;
            _solutionRewriter = new SolutionRewriter(projectResult, solutionConfiguration.ToList<ProjectConfiguration>());
        }
        private void InitSolutionRewriter(List<AnalyzerResult> analyzerResults, List<PortCoreConfiguration> solutionConfiguration)
        {
            CheckCache();
            InitRules(solutionConfiguration, analyzerResults);
            _solutionRewriter = new SolutionRewriter(analyzerResults, solutionConfiguration.ToList<ProjectConfiguration>());
        }

        private void DownloadRecommendationFiles(HashSet<string> allReferences)
        {
            allReferences.Add(Constants.ProjectRecommendationFile);

            _portSolutionResult.References = allReferences.ToHashSet<string>();

            using var httpClient = new HttpClient();
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
                    catch (Exception)
                    {
                        //We are checking which files have a recommendation, some of them won't
                    }
                }
            });

            _portSolutionResult.DownloadedFiles = matchedFiles.ToHashSet<string>();
            LogHelper.LogInformation("Found recommendations for the below:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, matchedFiles.Distinct()));
        }

        private void InitRules(List<PortCoreConfiguration> solutionConfiguration, List<AnalyzerResult> analyzerResults)
        {
            using var projectTypeFeatureDetector = new FeatureDetector();

            analyzerResults = analyzerResults.Where(a => solutionConfiguration.Select(s => s.ProjectPath).Contains(a.ProjectResult?.ProjectFilePath)).ToList();
            _projectTypeFeatureResults = projectTypeFeatureDetector.DetectFeaturesInProjects(analyzerResults);

            var allReferences = new HashSet<string>();
            foreach (var projectConfiguration in solutionConfiguration)
            {
                var projectTypeFeatureResult = _projectTypeFeatureResults[projectConfiguration.ProjectPath];
                projectConfiguration.ProjectType = GetProjectType(projectTypeFeatureResult);
                if (projectConfiguration.UseDefaultRules)
                {
                    //If a rules dir was provided, copy files from that dir into the rules folder
                    if (!string.IsNullOrEmpty(projectConfiguration.RulesDir))
                    {
                        CopyOverrideRules(projectConfiguration.RulesDir);
                    }
                    projectConfiguration.RulesDir = Constants.RulesDefaultPath;
                    var projectResult = analyzerResults
                        .FirstOrDefault(a => a.ProjectResult?.ProjectFilePath == projectConfiguration.ProjectPath)
                        .ProjectResult;

                    projectResult?.SourceFileResults?.SelectMany(s => s.References)?.Select(r => r.Namespace).Distinct().ToList().ForEach(currentReference=> {
                            if (currentReference != null && !allReferences.Contains(currentReference))
                            {
                                allReferences.Add(currentReference);
                            }
                        });

                    projectResult?.SourceFileResults?.SelectMany(s => s.Children.OfType<UsingDirective>())?.Select(u=>u.Identifier).Distinct().ToList().ForEach(currentReference => {
                        if (currentReference != null && !allReferences.Contains(currentReference))
                        {
                            allReferences.Add(currentReference);
                        }
                    });
                }
            }
            DownloadRecommendationFiles(allReferences);

        }
        /// <summary>
        /// Initializes the Solution Port
        /// </summary>
        public SolutionResult AnalysisRun()
        {
            var solutionResult = _solutionRewriter.AnalysisRun();
            _portSolutionResult.AddSolutionResult(solutionResult);
            if (!string.IsNullOrEmpty(_solutionPath))
            {
                PortSolutionResultReportGenerator reportGenerator = new PortSolutionResultReportGenerator(_context, _portSolutionResult, _projectTypeFeatureResults);
                reportGenerator.GenerateAnalysisReport();

                LogHelper.LogInformation("Generating Post-Analysis Report");
                LogHelper.LogError($"{Constants.MetricsTag}: {reportGenerator.AnalyzeSolutionResultJsonReport}");
            }
            return solutionResult;
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
                LogHelper.LogError($"{Constants.MetricsTag}: {reportGenerator.PortSolutionResultJsonReport}");
            }
            return _portSolutionResult;
        }

        /// <summary>
        /// Runs an incremental actions analysis on files
        /// </summary>
        /// <param name="projectRules">The rules list to be used</param>
        /// <param name="updatedFiles">The files to be analyzed</param>
        /// <returns></returns>
        public List<IDEFileActions> RunIncremental(RootNodes projectRules, List<string> updatedFiles)
        {
            return _solutionRewriter.RunIncremental(projectRules, updatedFiles);
        }
        /// <summary>
        /// Runs an incremental actions analysis on a file
        /// </summary>
        /// <param name="projectRules">The rules list to be used</param>
        /// <param name="updatedFile">The file to be analyzed</param>
        /// <returns></returns>
        public List<IDEFileActions> RunIncremental(RootNodes projectRules, string updatedFile)
        {
            return _solutionRewriter.RunIncremental(projectRules, new List<string> { updatedFile });
        }



        private void CopyOverrideRules(string sourceDir)
        {
            var files = Directory.EnumerateFiles(sourceDir, "*.json").ToList();
            files.ForEach(file => {
                File.Copy(file, Path.Combine(Constants.RulesDefaultPath, Path.GetFileName(file)), true);
            });
        }

        private void CheckCache()
        {
            ResetCache();
            DownloadResourceFiles();            
        }

        public static void ResetCache()
        {
            try
            {
                var recommendationsTime = Directory.GetCreationTime(Constants.RulesDefaultPath);

                bool cleanRecommendations = recommendationsTime.AddHours(Constants.CacheExpiryHours) < DateTime.Now;

                if (cleanRecommendations)
                {
                    if (Directory.Exists(Constants.RulesDefaultPath))
                    {
                        Directory.Delete(Constants.RulesDefaultPath, true);
                    }
                    Directory.CreateDirectory(Constants.RulesDefaultPath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while deleting directory");
            }
        }



        private void DownloadResourceFiles()
        {
            Utils.DownloadFilesToFolder(Constants.S3TemplatesBucketUrl, Constants.ResourcesExtractedPath, Constants.TemplateFiles);
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
