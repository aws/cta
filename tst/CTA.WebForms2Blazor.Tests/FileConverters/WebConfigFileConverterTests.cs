using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.FileConverters
{
    [TestFixture]
    public class WebConfigFileConverterTests
    {
        [Test]
        public async Task TestWebConfigFileConverter()
        {
            FileConverter fc = new ConfigFileConverter(FileConverterSetupFixture.TestProjectPath, FileConverterSetupFixture.TestWebConfigFilePath);
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