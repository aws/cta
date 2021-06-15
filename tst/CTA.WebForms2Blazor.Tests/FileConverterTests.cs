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
        private string TestFilesDirectoryPath = Path.Combine("TestingArea", "TestFiles");

        //These are relative paths
        private string _testProjectPath;
        private string _testCodeFilePath;
        private string _testConfigFilePath;
        private string _testStaticFilePath;
        private string _testViewFilePath;
        private string _testProjectFilePath;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            _testCodeFilePath = Path.Combine(TestFilesDirectoryPath, "TestClassFile.cs");
            _testConfigFilePath = Path.Combine(TestFilesDirectoryPath, "SampleConfigFile.config");
            _testStaticFilePath = Path.Combine(TestFilesDirectoryPath, "SampleStaticFile.png");
            _testViewFilePath = Path.Combine(TestFilesDirectoryPath, "SampleViewFile.aspx");
            _testProjectFilePath = Path.Combine(TestFilesDirectoryPath, "SampleProjectFile.csproj");
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task TestStaticFileConverter()
        {
            FileConverter fc = new StaticFileConverter(_testStaticFilePath);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.First();
            byte[] bytes = fi.FileBytes;

            string fullPath = Path.Combine(_testProjectPath, _testStaticFilePath);

            Assert.IsTrue(bytes.Length == new FileInfo(fullPath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(Path.Combine("wwwroot", _testStaticFilePath)));
        }
    }
}
