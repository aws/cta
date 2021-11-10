using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Config;
using CTA.Rules.Models;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a solution
    /// </summary>
    public class SolutionRewriter
    {
        private readonly List<ProjectRewriter> _rulesRewriters;
        private readonly SolutionResult _solutionResult;

        /// <summary>
        /// Initializes a new instance of SolutionRewriter, analyzing the solution path using the provided config.
        /// WARNING: This constructor will rebuild and reanalyze the solution, which will have a performance impact. If you 
        /// have an already analyzed solution, use another constructor
        /// </summary>
        /// <param name="analyzerConfiguration">Configuration for code analyzer to be used (AnalyzerConfiguration)</param>
        /// <param name="solutionFilePath">Path to solution file</param>
        /// <param name="solutionConfiguration">Configuration for each project in solution to be built</param>
        public SolutionRewriter(string solutionFilePath, List<ProjectConfiguration> solutionConfiguration)
        {
            DownloadResourceFiles();
            _solutionResult = new SolutionResult();
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

            _rulesRewriters = new List<ProjectRewriter>();
            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(analyzerConfiguration, LogHelper.Logger);
            var analyzerResults = analyzer.AnalyzeSolution(solutionFilePath).Result;
            InitializeProjects(analyzerResults, solutionConfiguration);
        }

        /// <summary>
        /// Initializes a new instance of SolutionRewriter with an already analyzed solution
        /// </summary>
        /// <param name="analyzerResults">The solution analysis</param>
        /// <param name="solutionConfiguration">Configuration for each project in the solution</param>
        public SolutionRewriter(List<AnalyzerResult> analyzerResults, List<ProjectConfiguration> solutionConfiguration)
        {
            DownloadResourceFiles();
            _solutionResult = new SolutionResult();
            _rulesRewriters = new List<ProjectRewriter>();
            InitializeProjects(analyzerResults, solutionConfiguration);
        }

        public SolutionRewriter(IDEProjectResult projectResult, List<ProjectConfiguration> solutionConfiguration)
        {
            DownloadResourceFiles();
            _rulesRewriters = new List<ProjectRewriter>()
            {
                new ProjectRewriter(projectResult, solutionConfiguration.FirstOrDefault(s => s.ProjectPath == projectResult.ProjectPath))
            };
        }

        /// <summary>
        /// Initializes the SolutionRewriter
        /// </summary>
        public SolutionResult AnalysisRun()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_rulesRewriters, options, rulesRewriter =>
            {
                _solutionResult.ProjectResults.Add(rulesRewriter.Initialize());
            });
            return _solutionResult;
        }

        /// <summary>
        /// Run the SolutionRewriter using a previously created analysis
        /// </summary>
        public SolutionResult Run(Dictionary<string, ProjectActions> projectActions)
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_rulesRewriters, options, rulesRewriter =>
            {
                _solutionResult.ProjectResults.Add(rulesRewriter.Run(projectActions[rulesRewriter.RulesEngineConfiguration.ProjectPath]));
            });
            return _solutionResult;
        }

        public List<IDEFileActions> RunIncremental(RootNodes projectRules, List<string> updatedFiles)
        {
            var ideFileActions = new BlockingCollection<IDEFileActions>();
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_rulesRewriters, options, rulesRewriter =>
            {
                var result = rulesRewriter.RunIncremental(updatedFiles, projectRules);
                result.ForEach(fileAction => ideFileActions.Add(fileAction));
            });
            return ideFileActions.ToList();
        }

        /// <summary>
        /// Runs the solution rewriter after creating an analysis
        /// </summary>
        public SolutionResult Run()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_rulesRewriters, options, rulesRewriter =>
            {
                var projectResult = rulesRewriter.Run();
                var existingResult = _solutionResult.ProjectResults.FirstOrDefault(p => p.ProjectFile == projectResult.ProjectFile);
                if (existingResult == null)
                {
                    _solutionResult.ProjectResults.Add(projectResult);
                }
                else
                {
                    existingResult = projectResult;
                }
            });
            return _solutionResult;
        }

        /// <summary>
        /// Initializes a rewriter for each project in the solution that has a configuration
        /// </summary>
        /// <param name="analyzerResults">The list of analysis results for each project</param>
        /// <param name="solutionConfiguration">ProjectConfiguration for each project</param>
        private void InitializeProjects(List<AnalyzerResult> analyzerResults, List<ProjectConfiguration> solutionConfiguration)
        {
            foreach (var analyzerResult in analyzerResults)
            {
                var projectConfiguration = solutionConfiguration.Where(s => s.ProjectPath == analyzerResult.ProjectResult.ProjectFilePath).FirstOrDefault();
                if (projectConfiguration != null)
                {
                    ProjectRewriter rulesRewriter = new ProjectRewriter(analyzerResult, projectConfiguration);
                    _rulesRewriters.Add(rulesRewriter);
                }
            }
        }

        private void DownloadResourceFiles()
        {
            Utils.DownloadFilesToFolder(Constants.S3TemplatesBucketUrl, Constants.ResourcesExtractedPath, Constants.TemplateFiles);
        }
    }
}
