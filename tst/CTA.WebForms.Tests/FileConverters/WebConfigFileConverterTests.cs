using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms.FileConverters;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using NUnit.Framework;

namespace CTA.WebForms.Tests.FileConverters
{
    [TestFixture]
    public class WebConfigFileConverterTests
    {
        [Test]
        public async Task TestWebConfigFileConverter()
        {
            WebFormMetricContext metricContext = new WebFormMetricContext();
            FileConverter fc = new ConfigFileConverter(FileConverterSetupFixture.TestProjectPath, FileConverterSetupFixture.TestWebConfigFilePath, new TaskManagerService(), metricContext);
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var appSettingsContent = Encoding.UTF8.GetString(bytes);

            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "appsettings.json");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);

            Assert.True(appSettingsContent.Contains("UseMockData"));
            Assert.True(appSettingsContent.Contains("UseCustomizationData"));
            Assert.True(appSettingsContent.Contains("CatalogDBContext"));
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));

        }
    }
}