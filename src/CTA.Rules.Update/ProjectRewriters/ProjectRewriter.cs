using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using Codelyzer.Analysis.Model;
using CTA.Rules.Analyzer;
using CTA.Rules.Common.Helpers;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Models.Tokens;
using CTA.Rules.RuleFiles;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class ProjectRewriter
    {
        public ProjectConfiguration ProjectConfiguration;
        protected readonly List<RootUstNode> _sourceFileResults;
        protected readonly List<SourceFileBuildResult> _sourceFileBuildResults;
        protected readonly List<string> _projectReferences;
        protected readonly ProjectResult _projectResult;
        protected readonly List<string> _metaReferences;
        protected readonly AnalyzerResult _analyzerResult;
        protected readonly ProjectLanguage _projectLanguage;
        private IRulesAnalysis _rulesAnalyzer;

        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public ProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
        {
            _projectResult = new ProjectResult()
            {
                ProjectFile = projectConfiguration.ProjectPath,
                TargetVersions = projectConfiguration.TargetVersions,
                SourceVersions = projectConfiguration.SourceVersions,
                UpgradePackages = projectConfiguration.PackageReferences.Select(p => new PackageAction()
                {
                    Name = p.Key,
                    OriginalVersion = p.Value.Item1,
                    Version = p.Value.Item2
                }).ToList(),
                MissingMetaReferences = analyzerResult?.ProjectBuildResult?.MissingReferences
            };

            _analyzerResult = analyzerResult;
            _sourceFileBuildResults = analyzerResult?.ProjectBuildResult?.SourceFileBuildResults;
            _sourceFileResults = analyzerResult?.ProjectResult?.SourceFileResults;
            _projectReferences = analyzerResult?.ProjectBuildResult?.ExternalReferences?.ProjectReferences.Select(p => p.AssemblyLocation).ToList();
            _metaReferences = analyzerResult?.ProjectBuildResult?.Project?.MetadataReferences?.Select(m => m.Display).ToList()
                ?? projectConfiguration.MetaReferences;
            ProjectConfiguration = projectConfiguration;
            _projectLanguage = VisualBasicUtils.IsVisualBasicProject(ProjectConfiguration.ProjectPath) ? ProjectLanguage.VisualBasic : ProjectLanguage.Csharp;
        }

        public ProjectRewriter(IDEProjectResult projectResult, ProjectConfiguration projectConfiguration)
        {
            _sourceFileResults = projectResult.RootNodes;
            _sourceFileBuildResults = projectResult.SourceFileBuildResults;
            ProjectConfiguration = projectConfiguration;

            _projectResult = new ProjectResult()
            {
                ProjectFile = projectConfiguration.ProjectPath,
                TargetVersions = projectConfiguration.TargetVersions,
                SourceVersions = projectConfiguration.SourceVersions,
                UpgradePackages = projectConfiguration.PackageReferences.Select(p => new PackageAction()
                {
                    Name = p.Key,
                    OriginalVersion = p.Value.Item1,
                    Version = p.Value.Item2
                }).ToList()
            };
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
                var allReferences = _sourceFileResults?.SelectMany(s => s.References)
                        .Union(_sourceFileResults.SelectMany(s => s.Children.OfType<UsingDirective>())?.Select(u => new Reference() { Namespace = u.Identifier, Assembly = u.Identifier }).Distinct())
                        .Union(_sourceFileResults.SelectMany(s => s.Children.OfType<ImportsStatement>())?.Select(u => new Reference() {Namespace = u.Identifier, Assembly = u.Identifier }).Distinct())
                        .Union(ProjectConfiguration.AdditionalReferences.Select(r => new Reference { Assembly = r, Namespace = r }));
                RulesFileLoader rulesFileLoader = new RulesFileLoader(allReferences, ProjectConfiguration.RulesDir, ProjectConfiguration.TargetVersions, _projectLanguage, string.Empty, ProjectConfiguration.AssemblyDir);
                
                var projectRules = rulesFileLoader.Load();

                HashSet<NodeToken> projectTokens;
                if (_projectLanguage == ProjectLanguage.VisualBasic)
                {
                    _rulesAnalyzer = new VisualBasicRulesAnalysis(_sourceFileResults, projectRules.VisualBasicRootNodes,
                        ProjectConfiguration.ProjectType);
                    projectTokens = projectRules.VisualBasicRootNodes.ProjectTokens;
                }
                else
                {
                    _rulesAnalyzer = new RulesAnalysis(_sourceFileResults, projectRules.CsharpRootNodes, ProjectConfiguration.ProjectType);
                    projectTokens = projectRules.CsharpRootNodes.ProjectTokens;
                }
                
                _rulesAnalyzer = _projectLanguage == ProjectLanguage.VisualBasic ? new VisualBasicRulesAnalysis(_sourceFileResults, projectRules.VisualBasicRootNodes, ProjectConfiguration.ProjectType) : new
                    RulesAnalysis(_sourceFileResults, projectRules.CsharpRootNodes, ProjectConfiguration.ProjectType);
                projectActions = _rulesAnalyzer.Analyze();
                _projectReferences.ForEach(p =>
                {
                    projectActions.ProjectReferenceActions.Add(Config.Utils.GetRelativePath(ProjectConfiguration.ProjectPath, p));
                });

                _projectResult.ActionPackages = projectActions.PackageActions.Distinct().ToList();
                _projectResult.MetaReferences = _metaReferences;

                foreach (var p in ProjectConfiguration.PackageReferences)
                {
                    projectActions.PackageActions.Add(new PackageAction() { Name = p.Key, OriginalVersion = p.Value.Item1, Version = p.Value.Item2 });
                }
                MergePackages(projectActions.PackageActions);

                projectActions.ProjectLevelActions = projectTokens.SelectMany(p => p.ProjectTypeActions).Distinct().ToList();
                projectActions.ProjectLevelActions.AddRange(projectTokens.SelectMany(p => p.ProjectLevelActions).Distinct());
                projectActions.ProjectLevelActions.AddRange(projectTokens.SelectMany(p => p.ProjectFileActions).Distinct());

                projectActions.CsharpProjectRules = projectRules.CsharpRootNodes;
                projectActions.VbProjectRules = projectRules.VisualBasicRootNodes;
                _projectResult.ProjectActions = projectActions;

                _projectResult.FeatureType = ProjectConfiguration.ProjectType;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while initializing project {0}", ProjectConfiguration.ProjectPath);
            }

            return _projectResult;
        }

        /// <summary>
        /// Initializes the ProjectRewriter then runs it
        /// </summary>
        public virtual ProjectResult Run()
        {
            var projectResult = Initialize();
            return Run(projectResult.ProjectActions);
        }

        /// <summary>
        /// Runs the project rewriter using a previously initialized analysis
        /// </summary>
        /// <param name="projectActions"></param>
        public virtual ProjectResult Run(ProjectActions projectActions)
        {
            _projectResult.ProjectActions = projectActions;
            CodeReplacer baseReplacer = new CodeReplacer(_sourceFileBuildResults, ProjectConfiguration, _metaReferences, _analyzerResult, _projectLanguage, projectResult: _projectResult);
            _projectResult.ExecutedActions = baseReplacer.Run(projectActions, ProjectConfiguration.ProjectType);
            return _projectResult;
        }

        public virtual List<IDEFileActions> RunIncremental(List<string> updatedFiles, CsharpRootNodes projectRules)
        {
            var ideFileActions = new List<IDEFileActions>();

            var allReferences = _sourceFileResults?.SelectMany(s => s.References).Distinct();
            RulesFileLoader rulesFileLoader = new RulesFileLoader(allReferences, Constants.RulesDefaultPath, ProjectConfiguration.TargetVersions, _projectLanguage, string.Empty, ProjectConfiguration.AssemblyDir);
            projectRules = rulesFileLoader.Load().CsharpRootNodes;

            RulesAnalysis walker = new RulesAnalysis(_sourceFileResults, projectRules, ProjectConfiguration.ProjectType);
            var projectActions = walker.Analyze();

            CodeReplacer baseReplacer = new CodeReplacer(_sourceFileBuildResults, ProjectConfiguration, _metaReferences, _analyzerResult, _projectLanguage, updatedFiles, projectResult: _projectResult);
            _projectResult.ExecutedActions = baseReplacer.Run(projectActions, ProjectConfiguration.ProjectType);

            ideFileActions = projectActions
                .FileActions
                .SelectMany(f => f.NodeTokens.Select(n => new IDEFileActions() { TextSpan = n.TextSpan,  Description = n.Description, FilePath = f.FilePath, TextChanges = n.TextChanges }))
                .ToList();
            return ideFileActions;
        }


        /// <summary>
        /// Merges the packages from packageActions with the packages in the ProjectConfiguration
        /// </summary>
        /// <param name="packageActions">A list of packages and their versions to add to the project</param>
        private void MergePackages(BlockingCollection<PackageAction> packageActions)
        {
            if (ProjectConfiguration.PackageReferences != null)
            {
                foreach (var package in ProjectConfiguration.PackageReferences.Keys)
                {
                    var versionTuple = ProjectConfiguration.PackageReferences[package];
                    var version = versionTuple != null ? versionTuple.Item2 ?? "*" : "*";
                    packageActions.Add(new FilePackageAction() { Name = package, Version = version });
                }
            }
        }
    }

}
