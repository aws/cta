using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests
{
    [TestFixture]
    class FileInformationFactoryTests
    {
        private const string TEST_FILES_DIRECTORY_PATH = "TestingArea/TestFiles";

        private string _testProjectPath;
        private string testCodeFilePath;
        private string testConfigFilePath;
        private string testStaticFilePath;
        private string testViewFilePath;
        private string testProjectFilePath;

        private Factories.FileInformationFactory _fileFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string _testFilesPath = Path.Combine(_testProjectPath, TEST_FILES_DIRECTORY_PATH);
            testCodeFilePath = Path.Combine(_testFilesPath, "TestClassFile.cs");
            testConfigFilePath = Path.Combine(_testFilesPath, "SampleConfigFile.config");
            testStaticFilePath = Path.Combine(_testFilesPath, "SampleStaticFile.png");
            testViewFilePath = Path.Combine(_testFilesPath, "SampleViewFile.aspx");
            testProjectFilePath = Path.Combine(_testFilesPath, "SampleProjectFile.csproj");
        }

        [SetUp]
        public void Setup()
        {
            _fileFactory = new Factories.FileInformationFactory();
        }

        [Test]
        public void TestBuildBasic()
        {
            FileInformationModel.FileInformation codeFileObj = _fileFactory.Build(new FileInfo(testCodeFilePath), _testProjectPath);
            FileInformationModel.FileInformation configFileObj = _fileFactory.Build(new FileInfo(testConfigFilePath), _testProjectPath);
            FileInformationModel.FileInformation staticFileObj = _fileFactory.Build(new FileInfo(testStaticFilePath), _testProjectPath);
            FileInformationModel.FileInformation viewFileObj = _fileFactory.Build(new FileInfo(testViewFilePath), _testProjectPath);
            FileInformationModel.FileInformation projectFileObj = _fileFactory.Build(new FileInfo(testProjectFilePath), _testProjectPath);

            //Console.WriteLine(codeFileObj.GetType());
            //Console.WriteLine(configFileObj.GetType());
            //Console.WriteLine(staticFileObj.GetType());
            //Console.WriteLine(viewFileObj.GetType());

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
            files.Add(new FileInfo(testCodeFilePath));
            files.Add(new FileInfo(testConfigFilePath));
            files.Add(new FileInfo(testStaticFilePath));
            files.Add(new FileInfo(testViewFilePath));
            files.Add(new FileInfo(testProjectFilePath));

            List<FileInformationModel.FileInformation> fileObjects = _fileFactory.BuildMany(files, _testProjectPath).ToList();
            Assert.True(typeof(FileInformationModel.CodeFileInformation).IsInstanceOfType(fileObjects[0]));
            Assert.True(typeof(FileInformationModel.ConfigFileInformation).IsInstanceOfType(fileObjects[1]));
            Assert.True(typeof(FileInformationModel.StaticFileInformation).IsInstanceOfType(fileObjects[2]));
            Assert.True(typeof(FileInformationModel.ViewFileInformation).IsInstanceOfType(fileObjects[3]));
            Assert.True(typeof(FileInformationModel.ProjectFileInformation).IsInstanceOfType(fileObjects[4]));
        }
    }
}
