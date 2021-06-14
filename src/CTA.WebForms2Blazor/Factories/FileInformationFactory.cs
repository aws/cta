using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.Factories
{
    public class FileInformationFactory
    {
        private readonly string _sourceProjectPath;
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly WorkspaceManagerService _webFormsWorkspaceManager;

        public FileInformationFactory(
            string sourceProjectPath,
            WorkspaceManagerService blazorWorkspaceManager,
            WorkspaceManagerService webFormsWorkspaceManager)
        {
            _sourceProjectPath = sourceProjectPath;
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsWorkspaceManager = webFormsWorkspaceManager;
        }

        public FileInformation Build(FileInfo document)
        {
            // NOTE
            // Existing Type:   FileInfo = System.IO.FileInfo
            // Our New Type:    FileInformation = CTA.WebForms2Blazor.FileInformationModel.FileInformation

            // Add logic to determine the type of FileInformation
            // object to create, likely using the file type specified
            // in the FileInfo object

            string relativePath = Path.GetRelativePath(_sourceProjectPath, document.FullName);
            string extension = document.Extension;

            FileInformation fi;
            if (extension.Equals(".cs"))
            {
                fi = new CodeFileInformation(relativePath, _blazorWorkspaceManager, _webFormsWorkspaceManager);
            } else if (extension.Equals(".config"))
            {
                fi = new ConfigFileInformation(relativePath);
            } else if (extension.Equals(".aspx") || extension.Equals(".asax") || extension.Equals(".ascx"))
            {
                fi = new ViewFileInformation(relativePath);
            } else if (extension.Equals(".csproj"))
            {
                fi = new ProjectFileInformation(relativePath, _blazorWorkspaceManager, _webFormsWorkspaceManager);
            } else
            {
                fi = new StaticFileInformation(relativePath);
            }


            return fi;
        }

        public IEnumerable<FileInformation> BuildMany(IEnumerable<FileInfo> documents)
        {
            return documents.Select(document => Build(document));
        }
    }
}
