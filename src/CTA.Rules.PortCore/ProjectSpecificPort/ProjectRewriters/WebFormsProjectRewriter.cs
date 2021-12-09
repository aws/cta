using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Update;
using CTA.WebForms2Blazor;

namespace CTA.Rules.PortCore
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class WebFormsProjectRewriter : ProjectRewriter
    {
        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public WebFormsProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
            : base(analyzerResult, projectConfiguration)
        {
        }

        public WebFormsProjectRewriter(IDEProjectResult projectResult, ProjectConfiguration projectConfiguration)
            : base(projectResult, projectConfiguration)
        {
        }

        /// <summary>
        /// Initializes the ProjectRewriter then runs it
        /// </summary>
        public override ProjectResult Run()
        {
            var projectResult = Initialize();
            return Run(projectResult.ProjectActions);
        }

        /// <summary>
        /// Runs the project rewriter using a previously initialized analysis
        /// </summary>
        /// <param name="projectActions"></param>
        public override ProjectResult Run(ProjectActions projectActions)
        {
            // NOTE: project actions are not used, but are still used for telemetry
            _projectResult.ProjectActions = projectActions;

            if (ProjectConfiguration.PortCode)
            {
                RunWebFormsChanges();
            }

            return _projectResult;
        }

        public override List<IDEFileActions> RunIncremental(List<string> updatedFiles, RootNodes projectRules)
        {
            var ideFileActions = new List<IDEFileActions>();

            // Incremental porting for WebForms project is yet to be implemented
            
            return ideFileActions;
        }

        private void RunWebFormsChanges()
        {
            var projectDir = Path.GetDirectoryName(ProjectConfiguration.ProjectPath);
            var projectParentDir = Path.GetDirectoryName(projectDir);

            try
            {
                MigrationManager migrationManager = new MigrationManager(projectDir, _analyzerResult, ProjectConfiguration, _projectResult);
                Task.Run(() => migrationManager.PerformMigration()).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                LogHelper.LogError("WebForms Porting Error: Error while migrating WebForms to Blazor: ", e.Message);
            }
        }
    }
}
