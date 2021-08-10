using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class StaticResourceFileConverter : FileConverter
    {
        private readonly HostPageService _hostPageService;

        public StaticResourceFileConverter(string sourceProjectPath, string fullPath, HostPageService hostPageService, TaskManagerService taskManagerService)
            : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _hostPageService = hostPageService;
        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();

            // We want to add the relative path to _Host.cshtml before
            // prepending wwwroot/ because the web root folder is ignored
            // when fetching static files
            if (RelativePath.EndsWith(Constants.StyleSheetFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                _hostPageService.AddStyleSheetPath(RelativePath);
            }

            var newPath = Path.Combine(Constants.WebRootDirectoryName, RelativePath);
            var fullPath = Path.Combine(ProjectPath, RelativePath);

            FileInformation fi = new FileInformation(newPath, File.ReadAllBytes(fullPath));

            var fileList = new[] { fi };

            DoCleanUp();
            LogEnd();

            return Task.FromResult((IEnumerable<FileInformation>)fileList);
        }
    }
}
