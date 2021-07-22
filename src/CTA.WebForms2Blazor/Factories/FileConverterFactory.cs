using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.Factories
{
    public class FileConverterFactory
    {
        private readonly string _sourceProjectPath;
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly ProjectAnalyzer _webFormsProjectAnalyzer;
        private readonly ViewImportService _viewImportService;
        private readonly ClassConverterFactory _classConverterFactory;
        private readonly HostPageService _hostPageService;
        
        // TODO: Organize these into "types" and force
        // content separation in file system if it doesn't
        // already exist
        public readonly HashSet<string> StaticResourceExtensions = new HashSet<string>
        {
            ".jpeg", ".jpg", ".jif", ".jfif", ".gif", ".tif", ".tiff", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd",
            ".png", ".pdf", ".ico", ".css", ".map", ".eot", ".otf", ".svg", ".tff", ".woff", ".woff2", ".fnt",".fon",
            ".ttc", ".pfa", ".fot", ".sfd", ".vlw", ".pfb", ".etx", ".odttf", ".ttf"
        };

        public FileConverterFactory(
            string sourceProjectPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ViewImportService viewImportService,
            ClassConverterFactory classConverterFactory,
            HostPageService hostPageService)
        {
            _sourceProjectPath = sourceProjectPath;
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsProjectAnalyzer = webFormsProjectAnalyzer;
            _viewImportService = viewImportService;
            _classConverterFactory = classConverterFactory;
            _hostPageService = hostPageService;
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
            if (extension.Equals(Constants.CSharpCodeFileExtension))
            {
                fc = new CodeFileConverter(_sourceProjectPath, document.FullName, _blazorWorkspaceManager, _webFormsProjectAnalyzer, _classConverterFactory);
            }
            else if (extension.Equals(Constants.WebFormsConfigFileExtension))
            {
                fc = new ConfigFileConverter(_sourceProjectPath, document.FullName);
            }
            else if (extension.Equals(Constants.WebFormsPageMarkupFileExtension)
                || extension.Equals(Constants.WebFormsControlMarkupFileExtenion)
                || extension.Equals(Constants.WebFormsMasterPageMarkupFileExtension)
                || extension.Equals(Constants.WebFormsGlobalMarkupFileExtension))
            {
                fc = new ViewFileConverter(_sourceProjectPath, document.FullName, _viewImportService);
            }
            else if (extension.Equals(Constants.CSharpProjectFileExtension))
            {
                fc = new ProjectFileConverter(_sourceProjectPath, document.FullName, _blazorWorkspaceManager, _webFormsProjectAnalyzer);
            }
            else if (StaticResourceExtensions.Contains(extension))
            {
                fc = new StaticResourceFileConverter(_sourceProjectPath, document.FullName, _hostPageService);
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
