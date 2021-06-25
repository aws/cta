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
using CTA.Rules.Models;
using CTA.Rules.PortCore;
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
        private DownloadTestProjectsFixture WebForms2BlazorTestFixture;
        

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            WebForms2BlazorTestFixture = new DownloadTestProjectsFixture();
            WebForms2BlazorTestFixture.Setup();
            
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
            
            var ctaArgs = new[]
            {
                "-p", WebForms2BlazorTestFixture.EShopLegacyWebFormsProjectPath, // can hardcode for local use
                "-v", "net5.0",                // set the Target Framework version
                "-d", "true",                         // use the default rules files (these will get downloaded from S3 and will tell CTA which packages to add to the new .csproj file)
                "-m", "false",                        // this is the "mock run" flag. Setting it to false means rules will be applied if we do a full port.
            };

            // Handle argument assignment
            PortCoreRulesCli cli = new PortCoreRulesCli();
            cli.HandleCommand(ctaArgs);
            if (cli.DefaultRules)
            {
                // Since we're using default rules, we want to specify where to find those rules (once they are downloaded)
                cli.RulesDir = Constants.RulesDefaultPath;
            }
            
            var packageReferences = new Dictionary<string, Tuple<string, string>>
            {
                { "Autofac", new Tuple<string, string>("4.9.1.0", "4.9.3")},
                { "EntityFramework", new Tuple<string, string>("6.0.0.0", "6.4.4")},
                { "log4net", new Tuple<string, string>("2.0.8.0", "2.0.12")},
                { "Microsoft.Extensions.Logging.Log4Net.AspNetCore", new Tuple<string, string>("1.0.0", "2.2.12")}
            };
                
            // Create a configuration object using the CLI and other arbitrary values
            PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
            {
                ProjectPath = cli.FilePath,
                RulesDir = cli.RulesDir,
                IsMockRun = cli.IsMockRun,
                UseDefaultRules = cli.DefaultRules,
                PackageReferences = packageReferences,
                PortCode = false,
                PortProject = true,
                TargetVersions = new List<string> { cli.Version }
            };   
            
            _webFormsProjectAnalyzer = new ProjectAnalyzer(_testProjectPath, analyzerResult, projectConfiguration);
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
            FileInformation fi = fileList.Single();
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
            FileInformation fi = fileList.Single();
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
            FileInformation fi = fileList.Single();
            
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
            FileInformation fi = fileList.Single();

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
