using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.FileInformationModel;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileConverters;

namespace CTA.WebForms2Blazor.Tests
{
    [TestFixture]
    class FileConverterTests
    {
        

        //These are full paths
        private string _testProjectPath;
        private string _testFilesDirectoryPath;

        private string _testCodeFilePath;
        private string _testWebConfigFilePath;
        private string _testStaticFilePath;
        private string _testViewFilePath;
        private string _testProjectFilePath;
        private string _testAreaFullPath;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            _testFilesDirectoryPath = Path.Combine(_testProjectPath, Path.Combine("TestingArea", "TestFiles"));
            _testCodeFilePath = Path.Combine(_testFilesDirectoryPath, "TestClassFile.cs");
            _testWebConfigFilePath = Path.Combine(_testFilesDirectoryPath, "web.config");
            _testStaticFilePath = Path.Combine(_testFilesDirectoryPath, "SampleStaticFile.png");
            _testViewFilePath = Path.Combine(_testFilesDirectoryPath, "SampleViewFile.aspx");
            _testProjectFilePath = Path.Combine(_testFilesDirectoryPath, "SampleProjectFile.csproj");
            _testAreaFullPath = Path.Combine(_testProjectPath, _testFilesDirectoryPath);
        }

        [SetUp]
        public void Setup()
        {
            //Might be needed in the future for other file converters
        }

        [Test]
        public async Task TestStaticFileConverter()
        {
            FileConverter fc = new StaticFileConverter(_testProjectPath,  _testStaticFilePath);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.First();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(_testProjectPath, _testStaticFilePath);

            Assert.IsTrue(bytes.Length == new FileInfo(_testStaticFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(Path.Combine("wwwroot", relativePath)));
        }

        [Test]
        public async Task TestWebConfigFileConverter()
        {
            FileConverter fc = new ConfigFileConverter(_testProjectPath, _testWebConfigFilePath);
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.First();
            
            byte[] bytes = fi.FileBytes;
            var appSettingsContent = Encoding.UTF8.GetString(bytes);

            string newPath = Path.Combine(_testFilesDirectoryPath, "appsettings.json");
            string relativePath = Path.GetRelativePath(_testProjectPath, newPath);

            Assert.True(appSettingsContent.Contains("UseMockData"));
            Assert.True(appSettingsContent.Contains("UseCustomizationData"));
            Assert.True(appSettingsContent.Contains("CatalogDBContext"));
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));

        }
    }
}
