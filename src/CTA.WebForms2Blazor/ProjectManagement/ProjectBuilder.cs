using System.IO;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.ProjectManagement
{
    public class ProjectBuilder
    {
        private readonly string _outputProjectPath;

        public string OutputProjectPath { get { return _outputProjectPath; } }

        public ProjectBuilder(string outputProjectPath)
        {
            _outputProjectPath = outputProjectPath;
        }

        public void CreateRelativeDirectoryIfNotExists(string relativePath)
        {
            var fullPath = Path.Combine(_outputProjectPath, relativePath);

            // No exception is thrown if the directory already exists
            Directory.CreateDirectory(fullPath);
        }

        public void WriteFileInformationToProject(FileConverter newDocument)
        {
            WriteFileBytesToProject(newDocument.RelativePath, newDocument.GetFileBytes());
        }

        public void WriteFileBytesToProject(string relativePath, byte[] fileContent)
        {
            CreateRelativeDirectoryIfNotExists(Path.GetDirectoryName(relativePath));
            var fullPath = Path.Combine(_outputProjectPath, relativePath);

            if (File.Exists(fullPath))
            {
                // TODO: File already exists and will be overwritten, we
                // should log this
            }

            using (FileStream stream = File.Create(fullPath))
            {
                stream.Write(fileContent, 0, fileContent.Length);
            }

            // TODO: Log that file was created
        }
    }
}
