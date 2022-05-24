using CTA.WebForms.FileConverters;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms.Metrics;

namespace CTA.WebForms.Tests.FileConverters
{
    [TestFixture]
    class StaticResourceFileConverterTests
    {
        [Test]
        public async Task TestStaticResourceFileConverter()
        {
            WebFormMetricContext metricContext = new WebFormMetricContext();
            string sourceFilePath = FileConverterSetupFixture.TestStaticResourceFilePath;
            FileConverter fc = new StaticResourceFileConverter(FileConverterSetupFixture.TestProjectPath, sourceFilePath, new HostPageService(), new TaskManagerService(), metricContext);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, sourceFilePath);

            Assert.IsTrue(bytes.Length == new FileInfo(sourceFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(Path.Combine("wwwroot", relativePath)));
        }
    }
}

