using CTA.Rules.Models;
using CTA.Rules.ProjectFile;
using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on the project
    /// </summary>
    public class ProjectFileActions
    {
        public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, string> GetMigrateProjectFileAction(string empty)
        {
            Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, string> func
                = (string projectDir, ProjectType projectType, List<string> targetVersion, Dictionary<string, string> packageReferences, List<string> projectReferences) =>
            {
                ProjectFileCreator projectFileCreator = new ProjectFileCreator(projectDir, targetVersion,
                    packageReferences, projectReferences.ToList(), projectType);

                var result = projectFileCreator.Create();
                return result ? "Project file created" : string.Empty;
            };

            return func;
        }
    }
}
