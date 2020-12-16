using CTA.Rules.Analyzer;
using CTA.Rules.Config;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using CTA.Rules.RuleFiles;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using Codelyzer.Analysis.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class ProjectRewriter
    {
        public ProjectConfiguration RulesEngineConfiguration;
        private List<RootUstNode> _sourceFileResults;
        private List<SourceFileBuildResult> _sourceFileBuildResults;
        private List<string> _projectReferences;
        private ProjectResult _projectResult;

        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="rulesEngineConfiguration">ProjectConfiguration for this project</param>
        public ProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration rulesEngineConfiguration)
        {
            _projectResult = new ProjectResult() { ProjectFile = rulesEngineConfiguration.ProjectPath };

            _sourceFileBuildResults = analyzerResult?.ProjectBuildResult?.SourceFileBuildResults;
            _sourceFileResults = analyzerResult?.ProjectResult?.SourceFileResults;
            _projectReferences = analyzerResult?.ProjectBuildResult?.ExternalReferences?.ProjectReferences.Select(p => p.AssemblyLocation).ToList();            
            RulesEngineConfiguration = rulesEngineConfiguration;

        }

        /// <summary>
        /// Initializes the project rewriter by getting a list of actions that will be run
        /// </summary>
        /// <returns>A list of project actions to be run</returns>
        public ProjectActions Initialize()
        {
            ProjectActions projectActions = new ProjectActions();
            try
            {
                var allReferences = _sourceFileResults?.SelectMany(s => s.References).Distinct();
                RulesFileLoader rulesFileLoader = new RulesFileLoader(allReferences, RulesEngineConfiguration.RulesPath, RulesEngineConfiguration.TargetVersions, string.Empty, RulesEngineConfiguration.AssemblyDir);
                var result = rulesFileLoader.Load();

                RulesAnalysis walker = new RulesAnalysis(_sourceFileResults, result);
                projectActions = walker.Analyze();
                _projectReferences.ForEach(p =>
                {
                    projectActions.ProjectReferenceActions.Add(Config.Utils.GetRelativePath(RulesEngineConfiguration.ProjectPath, p));
                });

                foreach (var p in RulesEngineConfiguration.PackageReferences)
                {
                    projectActions.PackageActions.Add(new PackageAction() { Name = p.Key, Version = p.Value });
                }
                MergePackages(projectActions.PackageActions);
                projectActions.ProjectLevelActions = result.ProjectTokens.SelectMany(p => p.ProjectLevelActions).Distinct().ToList();
                projectActions.ProjectLevelActions.AddRange(result.ProjectTokens.SelectMany(p => p.ProjectFileActions));

                _projectResult.ProjectActions = projectActions;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while initializing project {0}", RulesEngineConfiguration.ProjectPath);
            }

            return projectActions;
        }

        /// <summary>
        /// Initializes the ProjectRewriter then runs it
        /// </summary>
        public ProjectResult Run()
        {
            ProjectActions projectActions = Initialize();
            return Run(projectActions);
        }

        /// <summary>
        /// Runs the project rewriter using a previously initialized analysis
        /// </summary>
        /// <param name="projectActions"></param>
        public ProjectResult Run(ProjectActions projectActions)
        {
            _projectResult.ProjectActions = projectActions;
            CodeReplacer baseReplacer = new CodeReplacer(_sourceFileBuildResults, RulesEngineConfiguration);
            _projectResult.ExecutedActions = baseReplacer.Run(projectActions, RulesEngineConfiguration.ProjectType);
            return _projectResult;
        }


        /// <summary>
        /// Merges the packages from packageActions with the packages in the ProjectConfiguration
        /// </summary>
        /// <param name="packageActions">A list of packages and their versions to add to the project</param>
        private void MergePackages(BlockingCollection<PackageAction> packageActions)
        {
            if(RulesEngineConfiguration.PackageReferences != null)
            {
                foreach(var package in RulesEngineConfiguration.PackageReferences.Keys)
                {
                    packageActions.Add(new FilePackageAction() { Name = package, Version = RulesEngineConfiguration.PackageReferences[package] });
                }
            }
        }
    }

}
