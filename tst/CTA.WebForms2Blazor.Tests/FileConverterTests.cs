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
        private const string TestFilesDirectoryPath = "TestingArea\\TestFiles";

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

            Assert.IsTrue(bytes.Length != 0);
            Assert.IsTrue(fi.RelativePath.Equals("wwwroot\\TestingArea\\TestFiles\\SampleStaticFile.png"));
        }
    }
}
