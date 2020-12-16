using CTA.Rules.Config;
using CTA.Rules.Models;
using System.Collections.Generic;
using System.IO;

namespace CTA.Rules.Actions
{
    public class FolderUpdate
    {
        private string _projectDir;
        private string _projectFile;
        private ProjectType _projectType;

        public FolderUpdate(string projectFile, ProjectType projectType)
        {
            _projectFile = projectFile;
            _projectDir = Directory.GetParent(_projectFile).FullName;
            _projectType = projectType;
        }

        //TODO Is there a better way to do this?
        /// <summary>
        /// Gets the main namespace in the project
        /// </summary>
        /// <param name="projectDir">The directory of the project</param>
        /// <returns></returns>
        private string GetProjectNamespace()
        {
            //This assumes the main namespace has not been changed (matches the project dir name):
            return Path.GetFileNameWithoutExtension(_projectFile);
        }
        public void Run()
        {

            if (_projectType == ProjectType.Mvc)
            {
                CreateMvcDirs(_projectDir);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
            }
            if (_projectType == ProjectType.WebApi)
            {
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
            }
        }
        /// <summary>
        /// Create files based on the project type
        /// </summary>
        /// <param name="projectDir">Directory of the project</param>
        /// <param name="projectType">Type of the project</param>
        /// <param name="fileType">Type of the file to be created</param>
        private void CreateStartupFiles(string projectDir, ProjectType projectType, FileTypeCreation fileType)
        {
            string projectNamespace = GetProjectNamespace();

            var file = string.Concat(projectDir, "/" + fileType.ToString() + ".cs");
            if(File.Exists(file))
            {
                File.Move(file, string.Concat(file, ".bak"));
            }
            File.WriteAllText(string.Concat(projectDir, "/" + fileType.ToString() + ".cs"), GetStartupFileContent(projectNamespace, projectType, fileType));

            LogChange(string.Format("Created " + fileType.ToString() + ".cs file using {0} template", projectType.ToString()));
        }

        /// <summary>
        /// Gets a Startup.cs file based on the project type
        /// </summary>
        /// <param name="projectNamespace">The project namespace</param>
        /// <param name="projectType">The project type</param>
        /// <param name="fileType">Type of the file to be retrieved</param>
        /// <returns>The content of the startup file</returns>
        private string GetStartupFileContent(string projectNamespace, ProjectType projectType, FileTypeCreation fileType)
        {
            return TemplateHelper.GetTemplateFileContent(projectNamespace, projectType, fileType.ToString() + ".cs");
        }

        /// <summary>
        /// Create Directory structure for MVC Projects
        /// </summary>
        /// <param name="projectDir">Directory of the project</param>
        private void CreateMvcDirs(string projectDir)
        {
            string wwwrootdir = string.Concat(projectDir, @"\wwwroot");
            string contentdir = string.Concat(projectDir, @"\Content");
            string scriptsdir = string.Concat(projectDir, @"\Scripts");

            Directory.CreateDirectory(wwwrootdir);

            LogChange(string.Format("Create wwwroot dir at {0}", projectDir));
            if (Directory.Exists(contentdir))
            {
                Directory.Move(contentdir, string.Concat(wwwrootdir, @"\Content"));
                LogChange(string.Format("Move Content folder to {0}", wwwrootdir));
            }
            if (Directory.Exists(scriptsdir))
            {
                Directory.Move(scriptsdir, string.Concat(wwwrootdir, @"\Scripts"));
                LogChange(string.Format("Move Scripts folder to {0}", wwwrootdir));
            }
        }


        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }
    }
}
