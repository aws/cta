using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms2Blazor;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class WebFormsProjectRewriter : ProjectRewriter
    {
        private ProjectConfiguration _projectConfiguration;

        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public WebFormsProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
            : base(analyzerResult, projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
        }

        public WebFormsProjectRewriter(IDEProjectResult projectResult, ProjectConfiguration projectConfiguration)
            : base(projectResult, projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
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
            // NOTE: project actions are not used, but are still used for telemetry
            _projectResult.ProjectActions = projectActions;

            if (ProjectConfiguration.PortCode)
            {
                RunWebFormsChanges();
            }

            return _projectResult;
        }

        public List<IDEFileActions> RunIncremental(List<string> updatedFiles, RootNodes projectRules)
        {
            var ideFileActions = new List<IDEFileActions>();

            // Incremental porting for WebForms project is yet to be implemented
            
            return ideFileActions;
        }

        private void RunWebFormsChanges()
        {
            var projectDir = Path.GetDirectoryName(ProjectConfiguration.ProjectPath);
            var projectParentDir = Path.GetDirectoryName(projectDir);
            var tempProjectDir = Path.Join(projectParentDir, string.Join("-", new DirectoryInfo(projectDir).Name, Path.GetRandomFileName()));
            try
            {
                var migrationManager = new MigrationManager(projectDir, tempProjectDir, "", _analyzerResult, _projectConfiguration, _projectResult);
                Task.Run(() => migrationManager.PerformMigration()).GetAwaiter().GetResult();

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
        }
    }
}
