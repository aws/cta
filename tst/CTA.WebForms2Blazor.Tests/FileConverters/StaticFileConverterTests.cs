using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.FileConverters
{
    [TestFixture]
    public class StaticFileConverterTests
    {
        [Test]
        public async Task TestStaticFileConverter()
        {
            WebFormMetricContext metricContext = new WebFormMetricContext();
            string sourceFilePath = FileConverterSetupFixture.TestStaticFilePath;
            FileConverter fc = new StaticFileConverter(FileConverterSetupFixture.TestProjectPath, sourceFilePath, new TaskManagerService(), metricContext);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, sourceFilePath);
            
            Assert.IsTrue(bytes.Length == new FileInfo(sourceFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));
        }
    }
}