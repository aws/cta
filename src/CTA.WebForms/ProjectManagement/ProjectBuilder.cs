using System;
using System.IO;
using System.Linq;
using CTA.Rules.Config;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Helpers;

namespace CTA.WebForms.ProjectManagement
{
    public class ProjectBuilder
    {
        private const string FileAlreadyExistsLogTemplate = "{0}: File at {1} Already Exists and Will be Overwritten";
        private const string FileInfoWrittenLogTemplate = "{0}: New File Information Was Written to {1}";

        private readonly string _outputProjectPath;

        public string OutputProjectPath { get { return _outputProjectPath; } }

        public ProjectBuilder(string outputProjectPath)
        {
            _outputProjectPath = outputProjectPath;
        }

        public void CreateRelativeDirectoryIfNotExists(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(_outputProjectPath, relativePath);

                // No exception is thrown if the directory already exists
                Directory.CreateDirectory(fullPath);
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Could not create relative directory {relativePath}.");
            }
        }

        public void WriteFileInformationToProject(FileInformation newDocument)
        {
            WriteFileBytesToProject(newDocument.RelativePath, newDocument.FileBytes);
        }

        public void WriteFileBytesToProject(string relativePath, byte[] fileContent)
        {
            var fullPath = Path.Combine(_outputProjectPath, relativePath);
            try
            {
                CreateRelativeDirectoryIfNotExists(Path.GetDirectoryName(relativePath));

                if (File.Exists(fullPath))
                {
                    LogHelper.LogInformation(string.Format(FileAlreadyExistsLogTemplate, GetType().Name, fullPath));
                    // TODO: Maybe copy and store the old version of this file somehow?
                }

                using (FileStream stream = File.Create(fullPath))
                {
                    stream.Write(fileContent, 0, fileContent.Length);
                }

                LogHelper.LogInformation(string.Format(FileInfoWrittenLogTemplate, GetType().Name, fullPath));
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Could not write file bytes to project (attempted to write to {fullPath}).");
            }
        }

        public void DeleteDirectoriesIfEmpty(string directory, string pathLimit)
        {
            if (directory == null || !Directory.Exists(directory))
            {
                return;
            }

            try
            {
                var beforePathLimit = FilePathHelper.IsSubDirectory(pathLimit, directory);
                var directoryEmpty = !Directory.EnumerateFileSystemEntries(directory).Any();

                if (beforePathLimit && directoryEmpty)
                {
                    var parentDir = Path.GetDirectoryName(directory);
                    Directory.Delete(directory);
                    DeleteDirectoriesIfEmpty(parentDir, pathLimit);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Could not delete directory at {directory}");
            }
        }

        public void DeleteFileAndEmptyDirectories(string filePath, string pathLimit)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var parentDir = Path.GetDirectoryName(filePath);
                File.Delete(filePath);
                DeleteDirectoriesIfEmpty(parentDir, pathLimit);
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Unable to delete file at {filePath}");
            }
        }
    }
}
