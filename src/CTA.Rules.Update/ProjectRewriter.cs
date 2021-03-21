using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using Codelyzer.Analysis.Model;
using CTA.Rules.Analyzer;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.RuleFiles;
using Microsoft.CodeAnalysis;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class ProjectRewriter
    {
        public ProjectConfiguration RulesEngineConfiguration;
        private readonly List<RootUstNode> _sourceFileResults;
        private readonly List<SourceFileBuildResult> _sourceFileBuildResults;
        private readonly List<string> _projectReferences;
        private readonly ProjectResult _projectResult;
        private readonly List<string> _metaReferences;

        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="rulesEngineConfiguration">ProjectConfiguration for this project</param>
        public ProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration rulesEngineConfiguration)
        {
            _projectResult = new ProjectResult()
            {
                ProjectFile = rulesEngineConfiguration.ProjectPath,
                TargetVersions = rulesEngineConfiguration.TargetVersions,
                UpgradePackages = rulesEngineConfiguration.PackageReferences.Select(p => new PackageAction()
                {
                    Name = p.Key,
                    OriginalVersion = p.Value.Item1,
                    Version = p.Value.Item2
                }).ToList()
            };

            _sourceFileBuildResults = analyzerResult?.ProjectBuildResult?.SourceFileBuildResults;
            _sourceFileResults = analyzerResult?.ProjectResult?.SourceFileResults;
            _projectReferences = analyzerResult?.ProjectBuildResult?.ExternalReferences?.ProjectReferences.Select(p => p.AssemblyLocation).ToList();
            _metaReferences = analyzerResult.ProjectBuildResult.Project.MetadataReferences.Select(m => m.Display).ToList();
            RulesEngineConfiguration = rulesEngineConfiguration;

        }

        /// <summary>
        /// Initializes the project rewriter by getting a list of actions that will be run
        /// </summary>
        /// <returns>A list of project actions to be run</returns>
        public ProjectResult Initialize()
        {
            ProjectActions projectActions = new ProjectActions();
            try
            {
                var allReferences = _sourceFileResults?.SelectMany(s => s.References).Distinct();
                RulesFileLoader rulesFileLoader = new RulesFileLoader(allReferences, RulesEngineConfiguration.RulesDir, RulesEngineConfiguration.TargetVersions, string.Empty, RulesEngineConfiguration.AssemblyDir);
                var result = rulesFileLoader.Load();

                RulesAnalysis walker = new RulesAnalysis(_sourceFileResults, result);
                projectActions = walker.Analyze();
                _projectReferences.ForEach(p =>
                {
                    projectActions.ProjectReferenceActions.Add(Config.Utils.GetRelativePath(RulesEngineConfiguration.ProjectPath, p));
                });

                _projectResult.ActionPackages = projectActions.PackageActions.Distinct().ToList();

                foreach (var p in RulesEngineConfiguration.PackageReferences)
                {
                    projectActions.PackageActions.Add(new PackageAction() { Name = p.Key, OriginalVersion = p.Value.Item1, Version = p.Value.Item2 });
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

            return _projectResult;
        }

        /// <summary>
        /// Initializes the ProjectRewriter then runs it
        /// </summary>
        public ProjectResult Run()
        {
            var projectResult = Initialize();
            return Run(projectResult.ProjectActions);
        }

        /// <summary>
        /// Runs the project rewriter using a previously initialized analysis
        /// </summary>
        /// <param name="projectActions"></param>
        public ProjectResult Run(ProjectActions projectActions)
        {
            _projectResult.ProjectActions = projectActions;
            CodeReplacer baseReplacer = new CodeReplacer(_sourceFileBuildResults, RulesEngineConfiguration, _metaReferences);
            _projectResult.ExecutedActions = baseReplacer.Run(projectActions, RulesEngineConfiguration.ProjectType);
            return _projectResult;
        }


        /// <summary>
        /// Merges the packages from packageActions with the packages in the ProjectConfiguration
        /// </summary>
        /// <param name="packageActions">A list of packages and their versions to add to the project</param>
        private void MergePackages(BlockingCollection<PackageAction> packageActions)
        {
            if (RulesEngineConfiguration.PackageReferences != null)
            {
                foreach (var package in RulesEngineConfiguration.PackageReferences.Keys)
                {
                    var versionTuple = RulesEngineConfiguration.PackageReferences[package];
                    var version = versionTuple != null ? versionTuple.Item2 ?? "*" : "*";
                    packageActions.Add(new FilePackageAction() { Name = package, Version = version });
                }
            }
        }
    }

}
