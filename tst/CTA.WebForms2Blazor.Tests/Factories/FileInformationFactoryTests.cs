using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;
using CTA.WebForms2Blazor.Factories;

namespace CTA.WebForms2Blazor.Tests.Factories
{
    [TestFixture]
    class FileInformationFactoryTests
    {
        private const string TestFilesDirectoryPath = "TestingArea/TestFiles";

        private string _testProjectPath;
        private string _testCodeFilePath;
        private string _testConfigFilePath;
        private string _testStaticFilePath;
        private string _testViewFilePath;
        private string _testProjectFilePath;

        private FileInformationFactory _fileFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var testFilesPath = Path.Combine(_testProjectPath, TestFilesDirectoryPath);
            _testCodeFilePath = Path.Combine(testFilesPath, "TestClassFile.cs");
            _testConfigFilePath = Path.Combine(testFilesPath, "SampleConfigFile.config");
            _testStaticFilePath = Path.Combine(testFilesPath, "SampleStaticFile.png");
            _testViewFilePath = Path.Combine(testFilesPath, "SampleViewFile.aspx");
            _testProjectFilePath = Path.Combine(testFilesPath, "SampleProjectFile.csproj");
        }

        [SetUp]
        public void Setup()
        {
            _fileFactory = new FileInformationFactory(_testProjectPath);
        }

        [Test]
        public void TestBuildBasic()
        {
            FileInformationModel.FileInformation codeFileObj = _fileFactory.Build(new FileInfo(_testCodeFilePath));
            FileInformationModel.FileInformation configFileObj = _fileFactory.Build(new FileInfo(_testConfigFilePath));
            FileInformationModel.FileInformation staticFileObj = _fileFactory.Build(new FileInfo(_testStaticFilePath));
            FileInformationModel.FileInformation viewFileObj = _fileFactory.Build(new FileInfo(_testViewFilePath));
            FileInformationModel.FileInformation projectFileObj = _fileFactory.Build(new FileInfo(_testProjectFilePath));

            Assert.True(typeof(FileInformationModel.CodeFileInformation).IsInstanceOfType(codeFileObj));
            Assert.True(typeof(FileInformationModel.ConfigFileInformation).IsInstanceOfType(configFileObj));
            Assert.True(typeof(FileInformationModel.StaticFileInformation).IsInstanceOfType(staticFileObj));
            Assert.True(typeof(FileInformationModel.ViewFileInformation).IsInstanceOfType(viewFileObj));
            Assert.True(typeof(FileInformationModel.ProjectFileInformation).IsInstanceOfType(projectFileObj));

        }

        [Test]
        public void TestBuildManyBasic()
        {
            List<FileInfo> files = new List<FileInfo>();
            files.Add(new FileInfo(_testCodeFilePath));
            files.Add(new FileInfo(_testConfigFilePath));
            files.Add(new FileInfo(_testStaticFilePath));
            files.Add(new FileInfo(_testViewFilePath));
            files.Add(new FileInfo(_testProjectFilePath));

            List<FileInformationModel.FileInformation> fileObjects = _fileFactory.BuildMany(files).ToList();
            Assert.True(typeof(FileInformationModel.CodeFileInformation).IsInstanceOfType(fileObjects[0]));
            Assert.True(typeof(FileInformationModel.ConfigFileInformation).IsInstanceOfType(fileObjects[1]));
            Assert.True(typeof(FileInformationModel.StaticFileInformation).IsInstanceOfType(fileObjects[2]));
            Assert.True(typeof(FileInformationModel.ViewFileInformation).IsInstanceOfType(fileObjects[3]));
            Assert.True(typeof(FileInformationModel.ProjectFileInformation).IsInstanceOfType(fileObjects[4]));
        }
    }
}
