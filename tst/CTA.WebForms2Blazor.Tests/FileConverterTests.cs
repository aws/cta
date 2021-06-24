using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.FileInformationModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.ProjectManagement;
using Microsoft.Extensions.Logging;
using CTA.Rules.Test;

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

        private ProjectAnalyzer _webFormsProjectAnalyzer;
        private WorkspaceManagerService _blazorWorkspaceManager;
        
        [SetUpFixture]
        public class WebForms2BlazorTestFixture : AwsRulesBaseTest
        {
            public static string TempDir;
            public static string CopyFolder;
            public static string DownloadLocation;
            public static string EShopOnBlazorSolutionPath;
            public static string EShopOnBlazorSolutionFilePath;
            public static string EShopLegacyWebFormsProjectPath;
       
            [OneTimeSetUp]
            public void Setup()
            {
                Setup(GetType());
                TempDir = GetTstPath(Path.Combine(new [] { "Projects", "Temp", "W2B" }));
                DownloadTestProjects();
       
                EShopOnBlazorSolutionFilePath = CopySolutionFolderToTemp("eShopOnBlazor.sln", TempDir);
                EShopOnBlazorSolutionPath = Directory.GetParent(EShopOnBlazorSolutionFilePath).FullName;

                CopyFolder = Directory.GetParent(EShopOnBlazorSolutionPath).FullName;
                //var tempHelp = Directory.EnumerateFiles(EShopOnBlazorProjectPath, "*.csproj", SearchOption.AllDirectories);
                
                EShopLegacyWebFormsProjectPath = Directory
                    .EnumerateFiles(EShopOnBlazorSolutionPath, "*.csproj", SearchOption.AllDirectories)
                    .First(filePath =>
                        filePath.EndsWith("eShopLegacyWebForms.csproj", StringComparison.InvariantCultureIgnoreCase));
                
            }
       
            private void DownloadTestProjects()
            {
                var tempDirectory = Directory.CreateDirectory(TempDir);
                DownloadLocation = Path.Combine(tempDirectory.FullName, "d");
       
                var fileName = Path.Combine(tempDirectory.Parent.FullName, @"TestProjects.zip");
                Utils.SaveFileFromGitHub(fileName, GithubInfo.TestGithubOwner, GithubInfo.TestGithubRepo, GithubInfo.TestGithubTag);
                ZipFile.ExtractToDirectory(fileName, DownloadLocation, true);
            }
       
            [OneTimeTearDown]
            public void Cleanup()
            {
                DeleteDir(0);
            }
       
            private void DeleteDir(int retries)
            {
                if (retries <= 10)
                {
                    try
                    {
                        Directory.Delete(TempDir, true);
                        Directory.Delete(CopyFolder, true);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(60000);
                        DeleteDir(retries + 1);
                    }
                }
            }
        }

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
            
            var codeAnalyzer = CreateDefaultCodeAnalyzer();
            // Get analyzer results from codelyzer (syntax trees, semantic models, package references, project references, etc)
            var analyzerResult = codeAnalyzer.AnalyzeProject(WebForms2BlazorTestFixture.EShopLegacyWebFormsProjectPath).Result;
            
            _webFormsProjectAnalyzer = new ProjectAnalyzer(_testProjectPath, analyzerResult);
            _blazorWorkspaceManager = new WorkspaceManagerService();
            
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
        
        private CodeAnalyzer CreateDefaultCodeAnalyzer()
        {
            // Create a basic logger
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
            var logger = loggerFactory.CreateLogger("");

            // Set up analyzer config
            var configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings = {GenerateJsonOutput = false},
                MetaDataSettings =
                {
                    ReferenceData = true,
                    LoadBuildData = true,
                    GenerateBinFiles = true,
                    LocationData = false,
                    MethodInvocations = false,
                    LiteralExpressions = false,
                    LambdaMethods = false,
                    DeclarationNodes = false,
                    Annotations = false,
                    InterfaceDeclarations = false,
                    EnumDeclarations = false,
                    StructDeclarations = false,
                    ReturnStatements = false,
                    InvocationArguments = false,
                    ElementAccess = false,
                    MemberAccess = false
                }
            };

            return CodeAnalyzerFactory.GetAnalyzer(configuration, logger);
        }

        [Test]
        public async Task TestProjectFileConverter()
        {
            FileConverter fc = new ProjectFileConverter(WebForms2BlazorTestFixture.EShopOnBlazorSolutionPath,
                WebForms2BlazorTestFixture.EShopLegacyWebFormsProjectPath,
                _blazorWorkspaceManager, _webFormsProjectAnalyzer);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.First();

            byte[] bytes = fi.FileBytes;
            var projectFileContents = Encoding.UTF8.GetString(bytes);
            string relativePath = Path.GetRelativePath(WebForms2BlazorTestFixture.EShopOnBlazorSolutionPath, WebForms2BlazorTestFixture.EShopLegacyWebFormsProjectPath);
            
            Assert.True(fi.RelativePath.Equals(relativePath));
            Assert.True(projectFileContents.Contains("PackageReference"));
            Assert.True(projectFileContents.Contains("EntityFramework"));
            Assert.True(projectFileContents.Contains("log4net"));
            Assert.True(projectFileContents.Contains("Microsoft.NET.Sdk.Web"));
            Assert.True(projectFileContents.Contains("Autofac"));
        }
    }
}
