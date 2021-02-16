using System.IO;
using CTA.Rules.Config;
using CTA.Rules.Models;

namespace CTA.Rules.Actions
{
    public class FolderUpdate
    {
        private readonly string _projectDir;
        private readonly string _projectFile;
        private readonly ProjectType _projectType;

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
        public string Run()
        {
            string runResult = string.Empty;
            if (_projectType == ProjectType.Mvc)
            {
                CreateMvcDirs(_projectDir);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
                runResult = "Mvc project detected. Created static files, Program.cs, and Startup.cs";
            }
            if (_projectType == ProjectType.WebApi)
            {
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
                runResult = "Web API project detected. Created Program.cs and Startup.cs";
            }
            return runResult;
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

            var file = Path.Combine(projectDir, string.Concat(fileType.ToString(), ".cs"));
            if (File.Exists(file))
            {
                File.Move(file, string.Concat(file, ".bak"));
            }
            File.WriteAllText(file, GetStartupFileContent(projectNamespace, projectType, fileType));

            LogChange(string.Format("Created {0}.cs file using {1} template", fileType.ToString(), projectType.ToString()));
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
            string wwwrootdir = Path.Combine(projectDir, Constants.Wwwroot);
            string contentdir = string.Concat(projectDir, Constants.Content);
            string scriptsdir = string.Concat(projectDir, Constants.Scripts);

            string targetContentDir = string.Concat(wwwrootdir, Constants.Content);
            string targetScriptsDir = string.Concat(wwwrootdir, Constants.Scripts);

            Directory.CreateDirectory(wwwrootdir);

            LogChange(string.Format("Create {0} dir at {1}", Constants.Wwwroot, projectDir));
            if (Directory.Exists(contentdir) && !Directory.Exists(targetContentDir))
            {
                Directory.Move(contentdir, targetContentDir);
                LogChange(string.Format("Move {0} folder to {1}", Constants.Content, wwwrootdir));
            }
            if (Directory.Exists(scriptsdir) && !Directory.Exists(targetScriptsDir))
            {
                Directory.Move(scriptsdir, targetScriptsDir);
                LogChange(string.Format("Move {0} folder to {1}", Constants.Scripts, wwwrootdir));
            }
        }


        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }
    }
}
