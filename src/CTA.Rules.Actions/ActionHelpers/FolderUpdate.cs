using System.IO;
using CTA.Rules.Common.Helpers;
using CTA.Rules.Config;
using CTA.Rules.Models;

namespace CTA.Rules.Actions
{
    public class FolderUpdate
    {
        private readonly string _projectDir;
        private readonly string _projectFile;
        private readonly ProjectType _projectType;
        private readonly string _codeFileExtension;

        public FolderUpdate(string projectFile, ProjectType projectType)
        {
            _projectFile = projectFile;
            _projectDir = Directory.GetParent(_projectFile).FullName;
            _projectType = projectType;
            _codeFileExtension =
                VisualBasicUtils.IsVisualBasicProject(projectFile)
                    ? FileExtension.VisualBasic
                    : FileExtension.CSharp;
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
            LogChange(string.Format("Project type: {0}", _projectType.ToString()));
            if (_projectType == ProjectType.Mvc)
            {
                CreateMvcDirs(_projectDir);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
                runResult = $"Mvc project detected. Created static files, Program{_codeFileExtension} and Startup{_codeFileExtension}";
            }
            if (_projectType == ProjectType.WebApi)
            {
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
                runResult = $"Web API project detected. Created Program{_codeFileExtension} and Startup{_codeFileExtension}";
            }
            if (_projectType == ProjectType.WCFConfigBasedService)
            {
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
                runResult = $"WCF Config Based Service Project detected. Created Program{_codeFileExtension} and Startup{_codeFileExtension}";
            }
            if (_projectType == ProjectType.WCFCodeBasedService)
            {
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Program);
                CreateStartupFiles(_projectDir, _projectType, FileTypeCreation.Startup);
                runResult = $"WCF Code Based Service Project detected. Created Program{_codeFileExtension} and Startup{_codeFileExtension}";
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

            var file = Path.Combine(projectDir, string.Concat(fileType.ToString(), _codeFileExtension));
            if (File.Exists(file))
            {
                File.Move(file, string.Concat(file, FileExtension.Backup));
            }
            File.WriteAllText(file, GetStartupFileContent(projectNamespace, projectType, fileType, _codeFileExtension));

            LogChange(string.Format("Created {0}{2} file using {1} template", fileType.ToString(), projectType.ToString(), _codeFileExtension));
        }

        /// <summary>
        /// Gets a Startup.cs file based on the project type
        /// </summary>
        /// <param name="projectNamespace">The project namespace</param>
        /// <param name="projectType">The project type</param>
        /// <param name="fileType">Type of the file to be retrieved</param>
        /// <param name="fileExtension">Extension of file to be retrieved</param>
        /// <returns>The content of the startup file</returns>
        private string GetStartupFileContent(string projectNamespace, ProjectType projectType, FileTypeCreation fileType, string fileExtension)
        {
            return TemplateHelper.GetTemplateFileContent(projectNamespace, projectType, fileType.ToString() + fileExtension);
        }

        /// <summary>
        /// Create Directory structure for MVC Projects
        /// </summary>
        /// <param name="projectDir">Directory of the project</param>
        private void CreateMvcDirs(string projectDir)
        {
            string wwwrootdir = Path.Combine(projectDir, Constants.Wwwroot);
            string contentdir = Path.Combine(projectDir, Constants.Content);
            string scriptsdir = Path.Combine(projectDir, Constants.Scripts);

            string targetContentDir = Path.Combine(wwwrootdir, Constants.Content);
            string targetScriptsDir = Path.Combine(wwwrootdir, Constants.Scripts);

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
