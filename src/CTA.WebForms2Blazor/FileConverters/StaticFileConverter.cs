using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTA.Rules.Models;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class StaticFileConverter : FileConverter
    {
        private readonly WebFormMetricContext _metricsContext;
        private const string ChildActionType = "StaticFileConverter";

        public StaticFileConverter(string sourceProjectPath, string fullPath, TaskManagerService taskManagerService, WebFormMetricContext metricsContext)
            : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _metricsContext = metricsContext;
        }
        
        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectActionMetrics(WebFormsActionType.FileConversion, ChildActionType);
            FileInformation fi = new FileInformation(FilePathHelper.RemoveDuplicateDirectories(RelativePath), File.ReadAllBytes(FullPath));

            var fileList = new List<FileInformation>();
            fileList.Add(fi);

            DoCleanUp();
            LogEnd();

            return Task.FromResult((IEnumerable<FileInformation>)fileList);
        }
    }
}
