using System;
using System.Collections.Generic;
using System.IO;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Update;
using CTA.WebForms;
using Task = System.Threading.Tasks.Task;

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
               var result =  RunWebFormsChanges();
               if (result != null)
               {
                   _projectResult.WebFormsMetricResults = result.Metrics;
               }
            }

            return _projectResult;
        }

        public override List<IDEFileActions> RunIncremental(List<string> updatedFiles, RootNodes projectRules)
        {
            var ideFileActions = new List<IDEFileActions>();

            // Incremental porting for WebForms project is yet to be implemented
            LogHelper.LogError(new NotImplementedException($"{Constants.WebFormsErrorTag}Failed to run incremental porting on a WebForms project. This feature is not yet supported for this project type."));
            
            return ideFileActions;
        }

        private WebFormsPortingResult RunWebFormsChanges()
        {
            WebFormsPortingResult result = null;
            var projectDir = Path.GetDirectoryName(ProjectConfiguration.ProjectPath);

            try
            {
                MigrationManager migrationManager = new MigrationManager(projectDir, _analyzerResult, ProjectConfiguration, _projectResult);
                result = Task.Run(() => migrationManager.PerformMigration()).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Error while migrating WebForms to Blazor.");
            }
            return result;
        }
    }
}
