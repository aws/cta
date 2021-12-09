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
            
            return ideFileActions;
        }

        private WebFormsPortingResult RunWebFormsChanges()
        {
            WebFormsPortingResult result = null;
            var projectDir = Path.GetDirectoryName(ProjectConfiguration.ProjectPath);
            var projectParentDir = Path.GetDirectoryName(projectDir);
            var tempProjectDir = Path.Join(projectParentDir, string.Join("-", new DirectoryInfo(projectDir).Name, Path.GetRandomFileName()));
            try
            {
                var migrationManager = new MigrationManager(projectDir, tempProjectDir, "", _analyzerResult, ProjectConfiguration, _projectResult);
                result = Task.Run(() => migrationManager.PerformMigration()).GetAwaiter().GetResult();

                Directory.Delete(projectDir, true);
                while (Directory.Exists(projectDir))
                {
                    Thread.Sleep(0);
                }
                Directory.Move(tempProjectDir, projectDir);
            }
            catch (Exception e)
            {
                LogHelper.LogError("WebForms Porting Error: Error while migrating WebForms to Blazor: ", e.Message);
            }
            finally
            {
                if (Directory.Exists(tempProjectDir))
                {
                    Directory.Delete(tempProjectDir, true);
                }
            }

            return result;
        }
    }
}
