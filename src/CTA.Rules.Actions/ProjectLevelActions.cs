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
    public class ProjectLevelActions
    {
        public Func<string, ProjectType, string> GetArchiveFilesAction(string archiveFiles)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                bool isArchiveFiles = false;

                List<string> archived = new List<string>();
                List<string> deleted = new List<string>();

                projectDir = Directory.GetParent(projectDir).FullName;

                List<string> result = new List<string>();
                var filesToArchive = archiveFiles.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var fileToArchive in filesToArchive)
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(projectDir, fileToArchive, SearchOption.AllDirectories);
                    if (files != null && files.Count() > 0)
                    {
                        result.AddRange(files);
                    }
                }

                foreach (string s in result)
                {
                    isArchiveFiles = true;
                    if (!s.EndsWith(".bak"))
                    {
                        string backupFile = string.Concat(s, ".bak");
                        if (File.Exists(backupFile))
                        {
                            File.Delete(backupFile);
                            deleted.Add(backupFile);
                        }
                        File.Move(s, string.Concat(s, ".bak"));
                        archived.Add(s);
                    }
                }

                return string.Concat(
                    deleted.Count > 0 ? string.Concat("Deleted: ", string.Join(",", deleted)) : string.Empty,
                    archived.Count > 0 ? string.Concat("Archived: ", string.Join(",", archived)) : string.Empty);
            };

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet3FolderHierarchyAction(string empty)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                return folderUpdate.Run();
            };

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet5FolderHierarchyAction(string empty)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                return folderUpdate.Run();
            };

            return func;
        }

        public Func<string, ProjectType, string> GetMigrateConfigAction(string empty)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                ConfigMigrate configMigrate = new ConfigMigrate(projectDir, projectType);
                return configMigrate.Run();                 
            };

            return func;
        }
    }
}
