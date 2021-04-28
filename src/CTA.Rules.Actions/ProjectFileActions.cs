using System;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Models;
using CTA.Rules.ProjectFile;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on the project
    /// </summary>
    public class ProjectFileActions
    {
        public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, List<string>, string> GetMigrateProjectFileAction(string _)
        {
            static string func
(string projectDir, ProjectType projectType, List<string> targetVersion, Dictionary<string, string> packageReferences, List<string> projectReferences, List<string> metaReferences)
            {
                ProjectFileCreator projectFileCreator = new ProjectFileCreator(projectDir, targetVersion,
                    packageReferences, projectReferences.ToList(), projectType, metaReferences);

                var result = projectFileCreator.Create();
                return result ? "Project file created" : string.Empty;
            }
            return func;
        }
    }
}
