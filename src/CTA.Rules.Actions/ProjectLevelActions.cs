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
                StringBuilder logResult = new StringBuilder();

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
                            logResult.Append(string.Format("Deleted file {0}", backupFile));
                        }
                        File.Move(s, string.Concat(s, ".bak"));
                        logResult.Append(string.Format("Archived file {0}", s));
                    }
                }

                return logResult.ToString();
            };

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet3FolderHierarchyAction(string empty)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                folderUpdate.Run();
                return "";
            };

            return func;
        }

        public Func<string, ProjectType, string> GetCreateNet5FolderHierarchyAction(string empty)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
                folderUpdate.Run();
                return "";
            };

            return func;
        }

        public Func<string, ProjectType, string> GetMigrateConfigAction(string empty)
        {
            Func<string, ProjectType, string> func = (string projectDir, ProjectType projectType) =>
            {
                ConfigMigrate configMigrate = new ConfigMigrate(projectDir, projectType);
                configMigrate.Run();
                return "";
            };

            return func;
        }
        
        //public Func<SyntaxGenerator, Project, SyntaxNode, Project> GetAddDocumentAction(string fileName)
        //{
        //    Func<SyntaxGenerator, Project, SyntaxNode, Project> AddDocument = (SyntaxGenerator syntaxGenerator, Project project, SyntaxNode root) =>
        //    {
        //        if (root == null)
        //        {
        //            root = syntaxGenerator.ClassDeclaration("Test");
        //        }

        //        var solution = project.Solution;
        //        var workspace = solution.Workspace;

        //        string document = @"
        //            using System;
        //            namespace Temp
        //            {
        //                public static class Test
        //                {
        //                }
        //            }
        //            ";


        //        var dir = Directory.GetParent(project.FilePath);
        //    //project = project.AddDocument(fileName, root, filePath: string.Concat(dir, @"\", fileName));

        //        var doc = project.AddDocument("Test.cs", SyntaxFactory.ParseCompilationUnit(document), new string[] { "TestFolder" }, filePath: string.Concat(dir, @"\", fileName));

        //        var newSolution = doc.Project.Solution;

        //        bool result = workspace.TryApplyChanges(newSolution);





        //        return project;
        //    };
        //    return AddDocument;
        //}
    }
}
