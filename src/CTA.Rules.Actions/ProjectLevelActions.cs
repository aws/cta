using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on the project
    /// </summary>
    public class ProjectLevelActions
    {
        public Func<string, ProjectType, string> GetArchiveFilesAction(string archiveFiles)
        {
            string func(string projectDir, ProjectType projectType)
            {
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
            }

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet3FolderHierarchyAction(string _)
        {
            static string func(string projectDir, ProjectType projectType)
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                return folderUpdate.Run();
            }

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet5FolderHierarchyAction(string _)
        {
            static string func(string projectDir, ProjectType projectType)
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                return folderUpdate.Run();
            }

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet6FolderHierarchyAction(string _)
        {
            static string func(string projectDir, ProjectType projectType)
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                return folderUpdate.Run();
            }

            return func;
        }

        public Func<string, ProjectType, string> GetMigrateConfigAction(string _)
        {
            static string func(string projectDir, ProjectType projectType)
            {
                ConfigMigrate configMigrate = new ConfigMigrate(projectDir, projectType);
                return configMigrate.Run();
            }

            return func;
        }

        public Func<string, ProjectType, string> GetCreateMonolithServiceAction(string namespaceString, string projectName)
        {
            string func(string projectDir, ProjectType projectType)
            {
                AddMonolithFile(namespaceString, projectName, projectDir, projectType, FileTypeCreation.MonolithServiceMvc.ToString());
                return "";
            }
            return func;
        }

        public Func<string, ProjectType, string> GetCreateMonolithServiceWebAPIAction(string namespaceString, string projectName)
        {
            string func(string projectDir, ProjectType projectType)
            {
                AddMonolithFile(namespaceString, projectName, projectDir, projectType, FileTypeCreation.MonolithService.ToString());
                return "";
            }
            return func;
        }

        private void AddMonolithFile(string namespaceString, string projectName, string projectDir, ProjectType projectType, string templateFilename)
        {
            if (projectDir != null && projectDir.Length > 0)
            {
                if (string.Equals(Path.GetFileNameWithoutExtension(projectDir), projectName))
                {
                    UpdateCsprojReference(projectDir);
                    var pathOnly = Path.GetDirectoryName(projectDir);
                    var file = Path.Combine(pathOnly, string.Concat(templateFilename, ".cs"));
                    if (File.Exists(file))
                    {
                        if (File.Exists(string.Concat(file, ".bak")))
                        {
                            File.Delete(string.Concat(file, ".bak"));
                        }
                        File.Move(file, string.Concat(file, ".bak"));
                    }
                    File.WriteAllText(file, TemplateHelper.GetTemplateFileContent(namespaceString, ProjectType.MonolithService, templateFilename + ".cs"));
                    LogHelper.LogInformation(string.Format("Created {0}.cs file using {1} template", templateFilename, ProjectType.MonolithService.ToString()));

                }
            }

        }

        private void UpdateCsprojReference(string projectFile)
        {
            XmlDocument projectDoc = new XmlDocument();
            projectDoc.Load(projectFile);

            XmlNode newService = projectDoc.CreateNode(XmlNodeType.Element, "Compile", null);
            XmlNode attr = projectDoc.CreateNode(XmlNodeType.Attribute, "Include", null);
            attr.Value = Constants.MonolithService + ".cs";
            newService.Attributes.SetNamedItem(attr);

            var files = projectDoc.GetElementsByTagName("Compile");
            files.Item(0).ParentNode.InsertBefore(newService, files.Item(0));

            projectDoc.Save(projectFile);
        }
    }
}
