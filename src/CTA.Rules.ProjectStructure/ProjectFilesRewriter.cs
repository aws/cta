/*using Aws.Rules.Config;
using Aws.Rules.Models;
using Aws.Rules.ProjectFile;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Aws.Rules.ProjectStructure
{
    /// <summary>
    /// Runs changes on the project files and structure
    /// </summary>
    public class ProjectFilesRewriter
    {
        private List<string> _actions;
        private string _projectFile;
        private BlockingCollection<PackageAction> _packageAction;
        private List<string> _targetVersions;
        private BlockingCollection<string> _projectReferences;
        private bool _isAnalysisRun;

        /// <summary>
        /// Initializes an instance of ProjectFilesRewriter
        /// </summary>
        /// <param name="projectFile">The csproj file to run this on</param>
        /// <param name="targetVersions">The target versions to be added to the project file</param>
        /// <param name="packageAction">The package references to be added to the project file</param>
        public ProjectFilesRewriter(string projectFile, List<string> targetVersions, BlockingCollection<PackageAction> packageAction, BlockingCollection<string> projectReferences)
        {
            _projectFile = projectFile;
            _packageAction = packageAction;
            _targetVersions = targetVersions;
            _projectReferences = projectReferences;
            _actions = new List<string>();
        }

        /// <summary>
        /// Runs the ProjectFileRewriter
        /// </summary>
        public List<string> Run(bool isAnalysisRun = false)
        {
            _isAnalysisRun = isAnalysisRun;

            //Update csproj
            string projectDir = Directory.GetParent(_projectFile).FullName;

            ProjectType projectType = GetProjectType(_projectFile, projectDir);
            LogChange(string.Format("Detected project type {0}", projectType.ToString()));

            ConfigMigrate configMigrate = new ConfigMigrate(projectDir, projectType);
            _actions.AddRange(configMigrate.Run(_isAnalysisRun));

            ArchiveFiles(projectDir);

            FolderUpdate folderUpdate = new FolderUpdate(projectDir, projectType);
            _actions.AddRange(folderUpdate.Run(_isAnalysisRun));

            ProjectFileCreator projectFileCreator = new ProjectFileCreator(_projectFile, _targetVersions,
                _packageAction.ToList(), _projectReferences.ToList(), _isAnalysisRun, projectType);
            _actions.AddRange(projectFileCreator.Create());

            return _actions;
        }

        //TODO Use project guids, or add to codelyzer and send here
        /// <summary>
        /// Gets the type of the project based on folder structure
        /// </summary>
        /// <param name="projectFile"></param>
        /// <param name="projectDir"></param>
        /// <returns></returns>
        private ProjectType GetProjectType(string projectFile, string projectDir)
        {
            if (Directory.Exists(string.Concat(projectDir, @"\Content"))
                || Directory.Exists(string.Concat(projectDir, @"\Views"))
                || Directory.Exists(string.Concat(projectDir, @"\wwwroot")))
            {
                return ProjectType.Mvc;
            }
            else if ((Directory.Exists(string.Concat(projectDir, @"\Controllers"))))
            {         
                return ProjectType.WebApi;
            }
            else
            {
                return ProjectType.ClassLibrary;
            }
        }

        //TODO Is there a better way to do this?
        /// <summary>
        /// Archives known .NET Framework files
        /// </summary>
        /// <param name="projectDir">Directory of the project</param>
        private void ArchiveFiles(string projectDir)
        {
            List<string> result = new List<string>();

            foreach(var fileToArchive in Constants.filesToArchive)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(projectDir, fileToArchive, SearchOption.AllDirectories);
                if(files != null && files.Count() > 0)
                {
                    result.AddRange(files);
                }
            }

            foreach (string s in result)
            {
                if (!s.EndsWith(".bak"))
                {
                    try
                    {
                        if (!_isAnalysisRun)
                        {
                            string backupFile = string.Concat(s, ".bak");
                            if (File.Exists(backupFile))
                            {
                                File.Delete(backupFile);
                                LogChange(string.Format("Deleted file {0}", backupFile));
                            }
                            File.Move(s, string.Concat(s, ".bak"));
                        }
                        LogChange(string.Format("Archived file {0}", s));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError("Error archiving files: ");
                        LogHelper.LogError(ex.Message);
                    }
                }
            }
        }

        private void LogChange(string message)
        {
            _actions.Add(message);
            LogHelper.LogInformation(message);
        }
    }
}
*/
