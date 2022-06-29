using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms.FileConverters;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using NUnit.Framework;

namespace CTA.WebForms.Tests.FileConverters
{
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