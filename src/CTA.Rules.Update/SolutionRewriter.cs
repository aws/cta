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
        private readonly List<ProjectRewriter> _projectRewriters;
        private readonly SolutionResult _solutionResult;
        private readonly IProjectRewriterFactory _projectRewriterFactory;

        /// <summary>
        /// Initializes a new instance of SolutionRewriter, analyzing the solution path using the provided config.
        /// WARNING: This constructor will rebuild and reanalyze the solution, which will have a performance impact. If you 
        /// have an already analyzed solution, use another constructor
        /// </summary>
        /// <param name="analyzerConfiguration">Configuration for code analyzer to be used (AnalyzerConfiguration)</param>
        /// <param name="solutionFilePath">Path to solution file</param>
        /// <param name="solutionConfiguration">Configuration for each project in solution to be built</param>
        public SolutionRewriter(string solutionFilePath, List<ProjectConfiguration> solutionConfiguration, IProjectRewriterFactory projectRewriterFactory = null)
        {
            DownloadResourceFiles();
            _solutionResult = new SolutionResult();
            _projectRewriterFactory = projectRewriterFactory ?? new DefaultProjectRewriterFactory();
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

            _projectRewriters = new List<ProjectRewriter>();
            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(analyzerConfiguration, LogHelper.Logger);
            var analyzerResults = analyzer.AnalyzeSolution(solutionFilePath).Result;
            InitializeProjectRewriters(analyzerResults, solutionConfiguration);
        }

        /// <summary>
        /// Initializes a new instance of SolutionRewriter with an already analyzed solution
        /// </summary>
        /// <param name="analyzerResults">The solution analysis</param>
        /// <param name="solutionConfiguration">Configuration for each project in the solution</param>
        public SolutionRewriter(List<AnalyzerResult> analyzerResults, List<ProjectConfiguration> solutionConfiguration, IProjectRewriterFactory projectRewriterFactory = null)
        {
            DownloadResourceFiles();
            _solutionResult = new SolutionResult();
            _projectRewriterFactory = projectRewriterFactory ?? new DefaultProjectRewriterFactory();
            _projectRewriters = new List<ProjectRewriter>();
            InitializeProjectRewriters(analyzerResults, solutionConfiguration);
        }

        public SolutionRewriter(IDEProjectResult projectResult, List<ProjectConfiguration> solutionConfiguration, IProjectRewriterFactory projectRewriterFactory = null)
        {
            DownloadResourceFiles();
            _projectRewriterFactory = projectRewriterFactory ?? new DefaultProjectRewriterFactory();

            var projectConfiguration = solutionConfiguration.FirstOrDefault(s => s.ProjectPath == projectResult.ProjectPath);
            if (projectConfiguration != null)
            {
                var projectRewriter = _projectRewriterFactory.GetInstance(projectResult, projectConfiguration);
                _projectRewriters = new List<ProjectRewriter> {projectRewriter};
            }
        }

        /// <summary>
        /// Initializes the SolutionRewriter
        /// </summary>
        public SolutionResult AnalysisRun()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_projectRewriters, options, projectRewriter =>
            {
                _solutionResult.ProjectResults.Add(projectRewriter.Initialize());
            });
            return _solutionResult;
        }

        /// <summary>
        /// Run the SolutionRewriter using a previously created analysis
        /// </summary>
        public SolutionResult Run(Dictionary<string, ProjectActions> projectActions)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_projectRewriters, options, projectRewriter =>
            {
                var actionsToRun = projectActions[projectRewriter.ProjectConfiguration.ProjectPath];
                var projectResult = projectRewriter.Run(actionsToRun);
                _solutionResult.ProjectResults.Add(projectResult);
            });
            return _solutionResult;
        }

        public List<IDEFileActions> RunIncremental(RootNodes projectRules, List<string> updatedFiles)
        {
            var ideFileActions = new BlockingCollection<IDEFileActions>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_projectRewriters, options, projectRewriter =>
            {
                var result = projectRewriter.RunIncremental(updatedFiles, projectRules);
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
            Parallel.ForEach(_projectRewriters, options, projectRewriter =>
            {
                var projectResult = projectRewriter.Run();
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
        private void InitializeProjectRewriters(List<AnalyzerResult> analyzerResults, List<ProjectConfiguration> solutionConfiguration)
        {
            foreach (var analyzerResult in analyzerResults)
            {
                var projectConfiguration = solutionConfiguration.Where(s => s.ProjectPath == analyzerResult.ProjectResult.ProjectFilePath).FirstOrDefault();
                if (projectConfiguration != null)
                {
                    var projectRewriter = _projectRewriterFactory.GetInstance(analyzerResult, projectConfiguration);
                    _projectRewriters.Add(projectRewriter);
                }
            }
        }

        private void DownloadResourceFiles()
        {
            Utils.DownloadFilesToFolder(Constants.S3TemplatesBucketUrl, Constants.ResourcesExtractedPath, Constants.TemplateFiles);
        }
    }
}
