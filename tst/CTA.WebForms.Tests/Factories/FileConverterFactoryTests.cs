using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Codelyzer.Analysis;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using NUnit.Framework;
using CTA.WebForms.Factories;
using CTA.WebForms.Services;
using CTA.WebForms.FileConverters;
using CTA.WebForms.Metrics;
using CTA.WebForms.ProjectManagement;
using CTA.WebForms.Tests.ClassConverters;

namespace CTA.WebForms.Tests.Factories
{
    [TestFixture]
    class FileConverterFactoryTests
    {
        private readonly string TestFilesDirectoryPath = Path.Combine("TestingArea", "TestFiles");

        private string _testProjectPath;
        private string _testCodeFilePath;
        private string _testConfigFilePath;
        private string _testStaticFilePath;
        private string _testViewFilePath;
        private string _testProjectFilePath;

        private FileConverterFactory _fileConverterFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var testFilesPath = Path.Combine(_testProjectPath, TestFilesDirectoryPath);
            _testCodeFilePath = Path.Combine(testFilesPath, "TestClassFile.cs");
            _testConfigFilePath = Path.Combine(testFilesPath, "web.config");
            _testStaticFilePath = Path.Combine(testFilesPath, "SampleStaticFile.png");
            _testViewFilePath = Path.Combine(testFilesPath, "SampleViewFile.aspx");
            _testProjectFilePath = Path.Combine(testFilesPath, "SampleProjectFile.csproj");
        }

        [SetUp]
        public void Setup()
        {
            var webFormsProjectAnalyzer = new ProjectAnalyzer(_testProjectPath, new AnalyzerResult(), new PortCoreConfiguration(), new ProjectResult());
            var blazorWorkspaceManager = new WorkspaceManagerService();
            var taskManagerService = new TaskManagerService();

            blazorWorkspaceManager.CreateSolutionFile();
            WebFormMetricContext metricContext = new WebFormMetricContext();
            _fileConverterFactory = new FileConverterFactory(
                _testProjectPath,
                blazorWorkspaceManager,
                webFormsProjectAnalyzer,
                new ViewImportService(),
                new ClassConverterFactory(string.Empty, new LifecycleManagerService(), taskManagerService, metricContext),
                new HostPageService(),
                taskManagerService,
                metricContext);
        }

        [Test]
        public void TestBuildBasic()
        {
            FileConverter codeFileObj = _fileConverterFactory.Build(new FileInfo(_testCodeFilePath));
            FileConverter configFileObj = _fileConverterFactory.Build(new FileInfo(_testConfigFilePath));
            FileConverter staticFileObj = _fileConverterFactory.Build(new FileInfo(_testStaticFilePath));
            FileConverter viewFileObj = _fileConverterFactory.Build(new FileInfo(_testViewFilePath));
            FileConverter projectFileObj = _fileConverterFactory.Build(new FileInfo(_testProjectFilePath));

            Assert.True(typeof(CodeFileConverter).IsInstanceOfType(codeFileObj));
            Assert.True(typeof(ConfigFileConverter).IsInstanceOfType(configFileObj));
            Assert.True(typeof(StaticResourceFileConverter).IsInstanceOfType(staticFileObj));
            Assert.True(typeof(ViewFileConverter).IsInstanceOfType(viewFileObj));
            Assert.True(typeof(ProjectFileConverter).IsInstanceOfType(projectFileObj));

        }

        [Test]
        public void TestBuildManyBasic()
        {
            var files = new[]
            {
                new FileInfo(_testCodeFilePath),
                new FileInfo(_testConfigFilePath),
                new FileInfo(_testStaticFilePath),
                new FileInfo(_testViewFilePath),
                new FileInfo(_testProjectFilePath)
            };

            List<FileConverter> fileObjects = _fileConverterFactory.BuildMany(files).ToList();
            Assert.True(typeof(CodeFileConverter).IsInstanceOfType(fileObjects[0]));
            Assert.True(typeof(ConfigFileConverter).IsInstanceOfType(fileObjects[1]));
            Assert.True(typeof(StaticResourceFileConverter).IsInstanceOfType(fileObjects[2]));
            Assert.True(typeof(ViewFileConverter).IsInstanceOfType(fileObjects[3]));
            Assert.True(typeof(ProjectFileConverter).IsInstanceOfType(fileObjects[4]));
        }
    }
}
