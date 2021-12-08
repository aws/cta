using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class StaticResourceFileConverter : FileConverter
    {
        private readonly HostPageService _hostPageService;
        private readonly WebFormMetricContext _metricsContext;
        private const string ChildActionType = "StaticResourceFileConverter";

        public StaticResourceFileConverter(string sourceProjectPath, string fullPath, HostPageService hostPageService, TaskManagerService taskManagerService, WebFormMetricContext metricsContext)
            : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _hostPageService = hostPageService;
            _metricsContext = metricsContext;
        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectFileConversionMetrics(ChildActionType);
            // We want to add the relative path to _Host.cshtml before
            // prepending wwwroot/ because the web root folder is ignored
            // when fetching static files
            if (RelativePath.EndsWith(Constants.StyleSheetFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                _hostPageService.AddStyleSheetPath(RelativePath);
            }

            var newPath = FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.WebRootDirectoryName, RelativePath));
            var fullPath = Path.Combine(ProjectPath, RelativePath);

            FileInformation fi = new FileInformation(newPath, File.ReadAllBytes(fullPath));

            var fileList = new[] { fi };

            DoCleanUp();
            LogEnd();

            return Task.FromResult((IEnumerable<FileInformation>)fileList);
        }
    }
}
