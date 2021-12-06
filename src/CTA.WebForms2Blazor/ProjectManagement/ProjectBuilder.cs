using System;
using System.IO;
using CTA.Rules.Config;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.ProjectManagement
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
                LogHelper.LogError(e, $"Could not create relative directory {relativePath}.");
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
                LogHelper.LogError(e, $"Could not write file bytes to project (attempted to write to {fullPath}).");
            }
        }
    }
}
