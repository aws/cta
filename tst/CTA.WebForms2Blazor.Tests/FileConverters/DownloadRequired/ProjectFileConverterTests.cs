using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using CTA.Rules.Update;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.FileConverters.DownloadRequired
{
    [TestFixture]
    public class ProjectFileConverterTests
    {
        //These are full paths
        private string _testProjectPath;

        private ProjectAnalyzer _webFormsProjectAnalyzer;
        private WorkspaceManagerService _blazorWorkspaceManager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
   
            var codeAnalyzer = CreateDefaultCodeAnalyzer();
            // Get analyzer results from codelyzer (syntax trees, semantic models, package references, project references, etc)
            var analyzerResult = codeAnalyzer.AnalyzeProject(DownloadTestProjectsFixture.EShopLegacyWebFormsProjectPath).Result;
            
            var ctaArgs = new[]
            {
                "-p", DownloadTestProjectsFixture.EShopLegacyWebFormsProjectPath, // can hardcode for local use
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

            var projectRewriter = new ProjectRewriter(analyzerResult, projectConfiguration);
            ProjectResult projectResult = projectRewriter.Initialize();

            _webFormsProjectAnalyzer = new ProjectAnalyzer(_testProjectPath, analyzerResult, projectConfiguration, projectResult);
            _blazorWorkspaceManager = new WorkspaceManagerService();
        }

        [Test]
        public async Task TestProjectFileConverter()
        {
            WebFormMetricContext metricContext = new WebFormMetricContext();
            FileConverter fc = new ProjectFileConverter(DownloadTestProjectsFixture.EShopOnBlazorSolutionPath,
                DownloadTestProjectsFixture.EShopLegacyWebFormsProjectPath,
                _blazorWorkspaceManager, _webFormsProjectAnalyzer,
                new TaskManagerService(), metricContext);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();

            byte[] bytes = fi.FileBytes;
            var projectFileContents = Encoding.UTF8.GetString(bytes);
            string relativePath = Path.GetRelativePath(DownloadTestProjectsFixture.EShopOnBlazorSolutionPath, DownloadTestProjectsFixture.EShopLegacyWebFormsProjectPath);
            
            Assert.True(fi.RelativePath.Equals(relativePath));
            Assert.True(projectFileContents.Contains("PackageReference"));
            Assert.True(projectFileContents.Contains("EntityFramework"));
            Assert.True(projectFileContents.Contains("log4net"));
            Assert.True(projectFileContents.Contains("Microsoft.NET.Sdk.Web"));
            Assert.True(projectFileContents.Contains("Autofac"));
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
    }
}