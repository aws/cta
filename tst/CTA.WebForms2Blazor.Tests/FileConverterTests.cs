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
using CTA.Rules.Config;
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
        private string _testStaticResourceFilePath;
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
            _testStaticFilePath = Path.Combine(_testFilesDirectoryPath, "SampleStaticFile.csv");
            _testStaticResourceFilePath = Path.Combine(_testFilesDirectoryPath, "SampleStaticResourceFile.png");
            _testViewFilePath = Path.Combine(_testFilesDirectoryPath, "SampleViewFile.aspx");
            _testProjectFilePath = Path.Combine(_testFilesDirectoryPath, "SampleProjectFile.csproj");
            _testAreaFullPath = Path.Combine(_testProjectPath, _testFilesDirectoryPath);
            
            Utils.DownloadFilesToFolder(Constants.S3TemplatesBucketUrl, Constants.ResourcesExtractedPath, Constants.TemplateFiles);
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(Constants.ResourcesExtractedPath))
            {
                Directory.Delete(Constants.ResourcesExtractedPath, true);
            }
        }

        [SetUp]
        public void Setup()
        {
            //Might be needed in the future for other file converters
        }

        [Test]
        public async Task TestStaticResourceFileConverter()
        {
            FileConverter fc = new StaticResourceFileConverter(_testProjectPath,  _testStaticResourceFilePath);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.First();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(_testProjectPath, _testStaticResourceFilePath);

            Assert.IsTrue(bytes.Length == new FileInfo(_testStaticResourceFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(Path.Combine("wwwroot", relativePath)));
        }

        [Test]
        public async Task TestStaticFileConverter()
        {
            FileConverter fc = new StaticFileConverter(_testProjectPath, _testStaticFilePath);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.First();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(_testProjectPath, _testStaticFilePath);
            
            Assert.IsTrue(bytes.Length == new FileInfo(_testStaticFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));
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
