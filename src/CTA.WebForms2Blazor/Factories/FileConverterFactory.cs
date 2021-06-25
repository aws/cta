using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.Factories
{
    public class FileConverterFactory
    {
        private readonly string _sourceProjectPath;
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly ProjectAnalyzer _webFormsProjectAnalyzer;
        private readonly ClassConverterFactory _classConverterFactory;
        
        public readonly HashSet<string> StaticResourceExtensions = new HashSet<string>
        {
            ".jpeg", ".jpg", ".jif", ".jfif", ".gif", ".tif", ".tiff", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd",
            ".png", ".pdf", ".ico", ".css", ".map", ".eot", ".otf", ".svg", ".tff", ".woff", ".woff2", ".fnt",".fon",
            ".ttc", ".pfa", ".fot", ".sfd", ".vlw", ".pfb", ".etx", ".odttf"
        };

        public FileConverterFactory(
            string sourceProjectPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ClassConverterFactory classConverterFactory)
        {
            _sourceProjectPath = sourceProjectPath;
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsProjectAnalyzer = webFormsProjectAnalyzer;
            _classConverterFactory = classConverterFactory;
        }

        public FileConverter Build(FileInfo document)
        {
            // NOTE
            // Existing Type:   FileInfo = System.IO.FileInfo
            // Our New Type:    FileInformation = CTA.WebForms2Blazor.FileInformationModel.FileInformation

            // Add logic to determine the type of FileInformation
            // object to create, likely using the file type specified
            // in the FileInfo object

            string extension = document.Extension;

            FileConverter fc;
            if (extension.Equals(".cs"))
            {
                fc = new CodeFileConverter(_sourceProjectPath, document.FullName, _blazorWorkspaceManager, _webFormsProjectAnalyzer, _classConverterFactory);
            } else if (extension.Equals(".config"))
            {
                fc = new ConfigFileConverter(_sourceProjectPath, document.FullName);
            } else if (extension.Equals(".aspx") || extension.Equals(".asax") || extension.Equals(".ascx"))
            {
                fc = new ViewFileConverter(_sourceProjectPath, document.FullName);
            } else if (extension.Equals(".csproj"))
            {
                fc = new ProjectFileConverter(_sourceProjectPath, document.FullName, _blazorWorkspaceManager, _webFormsProjectAnalyzer);
            } else if (StaticResourceExtensions.Contains(extension))
            {
                fc = new StaticResourceFileConverter(_sourceProjectPath, document.FullName);
            }
            else
            {
                fc = new StaticFileConverter(_sourceProjectPath, document.FullName);
            }


            return fc;
        }

        public IEnumerable<FileConverter> BuildMany(IEnumerable<FileInfo> documents)
        {
            return documents.Select(document => Build(document));
        }
    }
}
