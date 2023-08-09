using System;
using System.IO;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms;

namespace CTA.Rules.Actions
{
    public class ProjectTypeActions
    {
        public Func<ProjectType, ProjectConfiguration, ProjectResult, AnalyzerResult, string> GetWebFormsPortingAction(string _)
        {
            string func(ProjectType projectType, ProjectConfiguration projectConfiguration, ProjectResult projectResult, AnalyzerResult analyzerResult)
            {
                if (projectType == ProjectType.WebForms)
                {
                    WebFormsPortingResult result = null;
                    var projectDir = Path.GetDirectoryName(projectConfiguration.ProjectPath);

                    try
                    {
                        MigrationManager migrationManager = new MigrationManager(projectDir, analyzerResult, projectConfiguration, projectResult);
                        result = Task.Run(() => migrationManager.PerformMigration()).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Error while migrating WebForms to Blazor.");
                    }

                    if (result != null)
                    {
                        projectResult.WebFormsMetricResults = result.Metrics;
                    }
                }

                return string.Empty;
            }

            return func;
        }
    }
}
