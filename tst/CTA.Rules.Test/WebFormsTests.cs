using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.Extensions.Logging;
using CTA.WebForms2Blazor;

namespace CTA.Rules.Test
{
    public class WebFormsTests : AwsRulesBaseTest
    {
        private string _tempDir = "";
        private string _downloadLocation;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _tempDir = Path.Combine(SetupTests.TempDir, "w2b_migration");
            _downloadLocation = Path.Combine(SetupTests.DownloadLocation, "w2b_migration");
        }

        [TestCase(TargetFramework.DotnetCoreApp31)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.Dotnet6)]
        public async Task WebForms2BlazorMigrationManagerTest(string targetFramework)
        {
            var projectName = "eShopLegacyWebForms";
            //var solutionPath = CopySolutionFolderToTemp("NAMEOFTESTSOLUTION.sln", _tempDir);
            //var projectPath = Directory.EnumerateFiles(Path.GetDirectoryName(solutionPath), $"{projectName}.csproj").First();
            var outputPath = Path.Combine(_tempDir, targetFramework);
            var solutionPath = @"D:\BuildableWebForms\eShopOnBlazor\eShopOnBlazor.sln";
            var projectPath = @"D:\BuildableWebForms\eShopOnBlazor\src\eShopLegacyWebForms\eShopLegacyWebForms.csproj";

            await MigrateProject(targetFramework, solutionPath, projectPath, outputPath);

            // Verify .csproj file changes
            var outputProjectFilePath = Path.Combine(outputPath, $"{projectName}.csproj");
            var portedProjectFileContent = await File.ReadAllTextAsync(outputProjectFilePath);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.12"" />", portedProjectFileContent);
            StringAssert.Contains(@"<PackageReference Include=""Autofac"" Version=""4.9.3"" />", portedProjectFileContent);
            StringAssert.Contains(@"<PackageReference Include=""EntityFramework"" Version=""6.4.4"" />", portedProjectFileContent);
            StringAssert.Contains(@"<PackageReference Include=""log4net"" Version=""2.0.12"" />", portedProjectFileContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.Extensions.Logging.Log4Net.AspNetCore"" Version=""2.2.12"" />", portedProjectFileContent);
            StringAssert.Contains(@"<PackageReference Include=""Fritz.BlazorWebformsComponents"" Version=""0.9.0"" />", portedProjectFileContent);

            // Verify host file
            var hostFile = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "_Host.cshtml"));
            StringAssert.Contains(@"<app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>", hostFile);

            // Verify view files
            var aboutRazor = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "About.razor"));
            var contactRazor = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "Contact.razor"));
            var defaultRazor = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "Default.razor"));
            var detailsRazor = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "Catalog", "Details.razor"));
            StringAssert.DoesNotContain("asp:Content", aboutRazor);
            StringAssert.Contains("@inherits eShopLegacyWebForms.About", aboutRazor);
            StringAssert.Contains(@"@page ""/About""", aboutRazor);
            StringAssert.DoesNotContain("<%#", aboutRazor);
            StringAssert.DoesNotContain("%>", aboutRazor);

            StringAssert.DoesNotContain("asp:Content", contactRazor); 
            StringAssert.Contains(@"@page ""/Contact""", contactRazor);
            StringAssert.Contains("@inherits eShopLegacyWebForms.Contact", contactRazor);
            StringAssert.DoesNotContain("<%#", contactRazor);
            StringAssert.DoesNotContain("%>", contactRazor);

            StringAssert.DoesNotContain("asp:Content", defaultRazor);
            StringAssert.Contains(@"@page ""/""", defaultRazor);
            StringAssert.Contains("@inherits eShopLegacyWebForms._Default", defaultRazor);
            StringAssert.Contains("GridView", defaultRazor);
            StringAssert.Contains("Columns", defaultRazor);
            StringAssert.Contains("BoundField", defaultRazor);
            StringAssert.Contains("ListView", defaultRazor);
            StringAssert.Contains("EmptyDataTemplate", defaultRazor);
            StringAssert.Contains("LayoutTemplate", defaultRazor);
            StringAssert.Contains("ItemTemplate", defaultRazor);
            StringAssert.DoesNotContain("<%#", defaultRazor);
            StringAssert.DoesNotContain("%>", defaultRazor);

            StringAssert.DoesNotContain("asp:Content", detailsRazor);
            StringAssert.Contains("@inherits eShopLegacyWebForms.Catalog.Details", detailsRazor);
            StringAssert.Contains(@"@page ""/Catalog/Details""", detailsRazor);
            StringAssert.DoesNotContain("<%#", detailsRazor);
            StringAssert.DoesNotContain("%>", detailsRazor);

            // Verify code-behind files
            var aboutRazorCs = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "About.razor.cs"));
            var contactRazorCs = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "Contact.razor.cs"));
            var defaultRazorCs = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "Default.razor.cs"));
            var detailsRazorCs = await File.ReadAllTextAsync(Path.Combine(outputPath, "Pages", "Catalog", "Details.razor.cs"));
            StringAssert.Contains("protected override void OnInitialized()", aboutRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", contactRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", defaultRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", detailsRazorCs);

            // Verify application files
            var startupText = await File.ReadAllTextAsync(Path.Combine(outputPath, "Startup.cs"));
            var programText = await File.ReadAllTextAsync(Path.Combine(outputPath, "Program.cs"));
            var webRequestInfoText = await File.ReadAllTextAsync(Path.Combine(outputPath, "WebRequestInfo.cs"));
            StringAssert.DoesNotContain("public class WebRequestInfo", startupText);
            
            StringAssert.Contains(
@"public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage(""/_Host"");
            });
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            // Code that runs on application startup
            // The following lines were extracted from Application_Start
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureContainer();
            ConfigDataBase();
            // This code replaces the original handling
            // of the BeginRequest event
            app.Use(async (context, next) =>
            {
                //set the property to our new object
                LogicalThreadContext.Properties[""activityid""] = new ActivityIdHelper();
                LogicalThreadContext.Properties[""requestinfo""] = new WebRequestInfo();
                _log.Debug(""Application_BeginRequest"");
                await next();
            });
        }", startupText);

            StringAssert.Contains(
                @"using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace eShopLegacyWebForms
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}", programText);

            StringAssert.Contains(
                @"using Autofac;
using Autofac.Integration.Web;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using log4net;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Web;
using Microsoft.AspNetCore.Components;

namespace eShopLegacyWebForms
{
    public class WebRequestInfo
    {
        public override string ToString()
        {
            return HttpContext.Current?.Request?.RawUrl + "", "" + HttpContext.Current?.Request?.UserAgent;
        }
    }
}", webRequestInfoText);
        }

        private async Task MigrateProject(string targetFramework, string solutionPath, string projectPath, string outputPath)
        {
            var csProjFilePath = @"D:\BuildableWebForms\eShopOnBlazor\src\eShopLegacyWebForms\eShopLegacyWebForms.csproj";
            var inputProjectDir = Path.GetDirectoryName(csProjFilePath);
            var outputProjectDir = Path.Combine(_tempDir, targetFramework);
            var slnPath = @"D:\BuildableWebForms\eShopOnBlazor\eShopOnBlazor.sln";

            var ctaArgs = new[]
            {
                "-p", csProjFilePath, // can hardcode for local use
                "-v", "net5.0",       // set the Target Framework version
                "-d", "true",         // use the default rules files (these will get downloaded from S3 and will tell CTA which packages to add to the new .csproj file)
                "-m", "false",        // this is the "mock run" flag. Setting it to false means rules will be applied if we do a full port.
            };

            // Handle argument assignment
            var cli = new PortCoreRulesCli();
            cli.HandleCommand(ctaArgs);

            var packageReferences = new Dictionary<string, Tuple<string, string>>
            {
                { "Autofac", new Tuple<string, string>("4.9.1.0", "4.9.3")},
                { "EntityFramework", new Tuple<string, string>("6.0.0.0", "6.4.4")},
                { "log4net", new Tuple<string, string>("2.0.8.0", "2.0.12")},
                { "Microsoft.Extensions.Logging.Log4Net.AspNetCore", new Tuple<string, string>("1.0.0", "2.2.12")},
                { "Fritz.BlazorWebformsComponents", new Tuple<string, string>("0.9.0", "0.9.0")}
            };

            // Create a configuration object using the CLI and other arbitrary values
            var projectConfiguration = new PortCoreConfiguration
            {
                ProjectPath = csProjFilePath,
                RulesDir = cli.RulesDir,
                IsMockRun = false,
                UseDefaultRules = true,
                PackageReferences = packageReferences,
                PortCode = false,
                PortProject = true,
                TargetVersions = new List<string> { targetFramework }
            };

            var codeAnalyzer = CreateDefaultCodeAnalyzer();
            var analyzerResult = codeAnalyzer.AnalyzeProject(csProjFilePath).Result;

            var solutionPort = new SolutionPort(slnPath, new List<AnalyzerResult>() { analyzerResult }, new List<PortCoreConfiguration>() { projectConfiguration });

            var migrationManager = new MigrationManager(inputProjectDir, outputProjectDir, analyzerResult, projectConfiguration);

            await migrationManager.PerformMigration();

        }

        private static CodeAnalyzer CreateDefaultCodeAnalyzer()
        {
            // Create a basic logger
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
            var logger = loggerFactory.CreateLogger("");
            LogHelper.Logger = logger;

            // Set up analyzer config
            var configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings = { GenerateJsonOutput = false },
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
