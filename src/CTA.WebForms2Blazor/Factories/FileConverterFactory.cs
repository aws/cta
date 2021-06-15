using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.Factories
{
    public class FileConverterFactory
    {
        private readonly string _sourceProjectPath;
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly WorkspaceManagerService _webFormsWorkspaceManager;

        public FileConverterFactory(
            string sourceProjectPath,
            WorkspaceManagerService blazorWorkspaceManager,
            WorkspaceManagerService webFormsWorkspaceManager)
        {
            _sourceProjectPath = sourceProjectPath;
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsWorkspaceManager = webFormsWorkspaceManager;
        }

        public FileConverter Build(FileInfo document)
        {
            // NOTE
            // Existing Type:   FileInfo = System.IO.FileInfo
            // Our New Type:    FileInformation = CTA.WebForms2Blazor.FileInformationModel.FileInformation

            // Add logic to determine the type of FileInformation
            // object to create, likely using the file type specified
            // in the FileInfo object

            string relativePath = Path.GetRelativePath(_sourceProjectPath, document.FullName);
            string extension = document.Extension;

            FileConverter fc;
            if (extension.Equals(".cs"))
            {
                fc = new CodeFileConverter(relativePath, _blazorWorkspaceManager, _webFormsWorkspaceManager);
            } else if (extension.Equals(".config"))
            {
                fc = new ConfigFileConverter(relativePath);
            } else if (extension.Equals(".aspx") || extension.Equals(".asax") || extension.Equals(".ascx"))
            {
                fc = new ViewFileConverter(relativePath);
            } else if (extension.Equals(".csproj"))
            {
                fc = new ProjectFileConverter(relativePath, _blazorWorkspaceManager, _webFormsWorkspaceManager);
            } else
            {
                fc = new StaticFileConverter(relativePath);
            }


            return fc;
        }

        public IEnumerable<FileConverter> BuildMany(IEnumerable<FileInfo> documents)
        {
            return documents.Select(document => Build(document));
        }
    }
}
