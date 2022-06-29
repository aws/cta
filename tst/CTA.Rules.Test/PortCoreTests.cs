using System;
using Codelyzer.Analysis;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis.Analyzer;

namespace CTA.Rules.Test
{
    public class PortCoreTests : AwsRulesBaseTest
    {
        public string tempDir;
        public string downloadLocation;

        [SetUp]
        public void Setup()
        {
            tempDir = SetupTests.TempDir;
            downloadLocation = SetupTests.DownloadLocation;
        }

        [Test]
        public void TestCli()
        {
            string mvcMusicStorePath = Directory.EnumerateFiles(tempDir, "MvcMusicStore.sln", SearchOption.AllDirectories).FirstOrDefault();
            string mvcMusicStoreProjectPath = Directory.EnumerateFiles(tempDir, "MvcMusicStore.csproj", SearchOption.AllDirectories).FirstOrDefault();
            string[] args = { "-p", mvcMusicStoreProjectPath, "-s", mvcMusicStorePath, "-r", Path.Combine(tempDir, "Input"), "-d", "true", "-a", "", "-v", "net5.0", "-m", "false" };
            var cli = new PortCoreRulesCli();
            cli.HandleCommand(args);
            Assert.NotNull(cli);
            Assert.NotNull(cli.FilePath);
            Assert.NotNull(cli.AssembliesDir);
            Assert.NotNull(cli.DefaultRules);
            Assert.NotNull(cli.IsMockRun);
            Assert.NotNull(cli.Version);


            string[] args2 = { "-s", mvcMusicStorePath, "-r", Path.Combine(tempDir, "Input"), "-d", "true", "-a", "", "-m", "true" };
            var cli2 = new PortCoreRulesCli();
            cli2.HandleCommand(args2);
        }

        [Test]
        public void TestSampleWebApi3Solution()
        {
            var solutionDir = TestWebApiApp(TargetFramework.DotnetCoreApp31);
            SolutionPort.ResetCache();
            TestWebApi(TargetFramework.Dotnet5, solutionDir);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        public void TestWebApi(string version, string solutionDir = "")
        {
            TestWebApiApp(version, solutionDir);
        }

        private string TestWebApiApp(string version, string solutionDir = "")
        {
            TestSolutionAnalysis results;

            if (string.IsNullOrEmpty(solutionDir))
            {
                results = AnalyzeSolution("SampleWebApi.sln", tempDir, downloadLocation, version);
            }
            else
            {
                results = AnalyzeSolution("SampleWebApi.sln", solutionDir, downloadLocation, version, null, true);
            }

            var analysisResult = results.ProjectResults.FirstOrDefault().ProjectAnalysisResult;

            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            //First run
            if (string.IsNullOrEmpty(solutionDir))
            {
                StringAssert.Contains("IActionResult", analysisResult);
                StringAssert.Contains("Startup", analysisResult);
                //Check that files have been created
                FileAssert.Exists(Path.Combine(projectDir, "Startup.cs"));
                FileAssert.Exists(Path.Combine(projectDir, "Program.cs"));
            }

            var homeControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "HouseController.cs"));

            //Check that namespace has been added
            StringAssert.Contains(@"Microsoft.AspNetCore.Mvc", homeControllerText);

            //Check that attribute has been added to class, and base class changed:
            StringAssert.Contains(@"[ApiController]", homeControllerText);
            StringAssert.Contains(@"ControllerBase", homeControllerText);

            //Check that invocation expression was replaced:
            StringAssert.Contains(@"this.Response.Headers.Add", homeControllerText);

            //Check that identifier as replaced:
            StringAssert.Contains(@"IActionResult", homeControllerText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.OData", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);

            //Check project actions
            var webApiProjectActions = results.SolutionRunResult.ProjectResults.First(p => p.ProjectFile.EndsWith("SampleWebApi.csproj"))
                .ExecutedActions.First(a => a.Key == "Project").Value;


            //When running it the first time
            if (string.IsNullOrEmpty(solutionDir))
            {
                Assert.AreEqual(4, webApiProjectActions.Count);
            }

            //When running the second time
            else
            {
                Assert.AreEqual(2, webApiProjectActions.Count);
            }
            return Directory.GetParent(results.SolutionRunResult.SolutionPath).FullName;
        }

        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestWebApiWithReferences(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("WebApiWithReferences.sln", tempDir);
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version);

            ValidateWebApiWithReferences(results);
        }

        [TestCase(TargetFramework.DotnetCoreApp31)]
        public async Task TestWebApiWithReferencesUsingGenerator(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("WebApiWithReferences.sln", tempDir);

            AnalyzerConfiguration configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings =
                {
                    GenerateJsonOutput = false,
                    OutputPath = @"/tmp/UnitTests"
                },

                MetaDataSettings =
                {
                    LiteralExpressions = true,
                    MethodInvocations = true,
                    Annotations = true,
                    DeclarationNodes = true,
                    LocationData = false,
                    ReferenceData = true,
                    LoadBuildData = true,
                    ElementAccess = true,
                    MemberAccess = true
                }
            };

            //CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(configuration, NullLogger.Instance);
            CodeAnalyzerByLanguage analyzer = new CodeAnalyzerByLanguage(configuration, NullLogger.Instance);

            SolutionPort solutionPort = new SolutionPort(solutionPath);

            var resultEnumerator = analyzer.AnalyzeSolutionGeneratorAsync(solutionPath).GetAsyncEnumerator();

            while (await resultEnumerator.MoveNextAsync())
            {
                using var result = resultEnumerator.Current;
                PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
                {
                    SolutionPath = solutionPath,
                    ProjectPath = result.ProjectResult.ProjectFilePath,
                    IsMockRun = false,
                    UseDefaultRules = true,
                    PortCode = true,
                    PortProject = true,
                    TargetVersions = new List<string> { version }
                };

                solutionPort.RunProject(result, projectConfiguration);
            }
            var portSolutionResult = solutionPort.GenerateResults();
            var testSolutionResult = GenerateSolutionResult(solutionPath, solutionPort.GetAnalysisResult(), portSolutionResult);

            ValidateWebApiWithReferences(testSolutionResult);
        }

        private void ValidateWebApiWithReferences(TestSolutionAnalysis results)
        {

            StringAssert.Contains("IActionResult", results.SolutionAnalysisResult);
            StringAssert.Contains("Startup", results.SolutionAnalysisResult);


            var webProjectFile = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("WebApiWithReferences.csproj")).FirstOrDefault();
            FileAssert.Exists(webProjectFile.CsProjectPath);
            var testClassLibraryProjectFile = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("TestReference.csproj")).FirstOrDefault();
            FileAssert.Exists(testClassLibraryProjectFile.CsProjectPath);
            var testClassLibrary2ProjectFile = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("TestReference2.csproj")).FirstOrDefault();
            FileAssert.Exists(testClassLibrary2ProjectFile.CsProjectPath);

            var webCsProjContent = webProjectFile.CsProjectContent;
            var classLibraryCsProjContent = testClassLibraryProjectFile.CsProjectContent;
            var classLibrary2CsProjContent = testClassLibrary2ProjectFile.CsProjectContent;

            string webProjectDir = Directory.GetParent(webProjectFile.CsProjectPath).FullName;

            var homeControllerText = File.ReadAllText(Path.Combine(webProjectDir, "Controllers", "HouseController.cs"));

            //Check that namespace has been added
            StringAssert.Contains(@"Microsoft.AspNetCore.Mvc", homeControllerText);

            //Check that attribute has been added to class, and base class changed:
            StringAssert.Contains(@"[ApiController]", homeControllerText);
            StringAssert.Contains(@"ControllerBase", homeControllerText);

            //Check that invocation expression was replaced:
            StringAssert.Contains(@"this.Response.Headers.Add", homeControllerText);

            //Check that identifier as replaced:
            StringAssert.Contains(@"IActionResult", homeControllerText);
            //Check that identifier as replaced:
            StringAssert.DoesNotContain(@"IHttpActionResult", homeControllerText);

            //Check that files have been created
            FileAssert.Exists(Path.Combine(webProjectDir, "Startup.cs"));
            FileAssert.Exists(Path.Combine(webProjectDir, "Program.cs"));

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.OData", webCsProjContent);

            //Check the framework references used
            Assert.True(webCsProjContent.IndexOf(@"Microsoft.NET.Sdk.Web") > 0);
            Assert.True(classLibraryCsProjContent.IndexOf("Microsoft.NET.Sdk") > 0);
            Assert.True(classLibrary2CsProjContent.IndexOf("Microsoft.NET.Sdk") > 0);

            //Check that projects are still referenced
            Assert.True(webCsProjContent.IndexOf(@"ProjectReference Include=") > 0);
            Assert.True(webCsProjContent.IndexOf(@"..\TestReference\TestReference.csproj") > 0);
            Assert.True(webCsProjContent.IndexOf(@"..\TestReference2\TestReference2.csproj""") > 0);

            Assert.True(webCsProjContent.IndexOf("Newtonsoft.Json") > 0);

            //Check project actions
            var classlibrary1Actions = results.SolutionRunResult.ProjectResults.First(p => p.ProjectFile.EndsWith("TestReference.csproj"))
                .ExecutedActions.First(a => a.Key == "Project").Value;
            var classlibrary2Actions = results.SolutionRunResult.ProjectResults.First(p => p.ProjectFile.EndsWith("TestReference2.csproj"))
                .ExecutedActions.First(a => a.Key == "Project").Value;
            var webApiProjectActions = results.SolutionRunResult.ProjectResults.First(p => p.ProjectFile.EndsWith("WebApiWithReferences.csproj"))
                .ExecutedActions.First(a => a.Key == "Project").Value;

            Assert.AreEqual(2, classlibrary1Actions.Count);
            Assert.AreEqual(2, classlibrary2Actions.Count);
            Assert.AreEqual(4, webApiProjectActions.Count);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMvcMusicStore(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("MvcMusicStore.sln", tempDir);
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version);

            ValidateMvcMusicStore(results, version);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMvcMusicStoreWithReferences(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("MvcMusicStore.sln", tempDir);
            var analyzerResults = GenerateSolutionAnalysis(solutionPath);

            var metaReferences = analyzerResults.ToDictionary(a => a.ProjectResult.ProjectFilePath, a => a.ProjectBuildResult.Project.MetadataReferences.Select(m => m.Display).ToList());
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version, metaReferences, true);

            ValidateMvcMusicStore(results, version);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMvcMusicStoreWithoutProjectPort(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("MvcMusicStore.sln", tempDir);
            var analyzerResults = GenerateSolutionAnalysis(solutionPath);

            var metaReferences = analyzerResults.ToDictionary(a => a.ProjectResult.ProjectFilePath, a => a.ProjectBuildResult.Project.MetadataReferences.Select(m => m.Display).ToList());
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version, metaReferences, true, false, false);

            ValidateMvcMusicStoreSolutionRunResultNodeTokenDeepClone(results);
        }

        private void ValidateMvcMusicStoreSolutionRunResultNodeTokenDeepClone(TestSolutionAnalysis results)
        {
            var fileActions = results.SolutionRunResult.ProjectResults.FirstOrDefault().ProjectActions.FileActions;
            // Before we change NodeToken Clone from shallow clone to deep
            // clone, all the items in FileActions list refers to the same
            // NodeToken object of a specific type, any TextChange object
            // being added to the NodeToken.TextChanges list through one
            // NodeToken reference in a FileAction object gets propogated
            // to all the other NodeToken references retained by other
            // FileAction objects. Thats why we were seeing 11 * 2 = 22
            // TextChange objects in each FileAction.NodeTokens.TextChanges
            // for this test project. Where:
            // 
            // 11 is the count of FileAction list
            // 2 is the count of TextChanges each FileAction.NodeTokens has
            // 
            // After updating the Clone method to deep clone, we are seeing
            // 2 TextChange objects in each FileAction.NodeTokens.TextChanges
            // list, which is correct and expected.
            var expectedNodeTokenTextChangesCounts = new Dictionary<string, int>()
            {
                ["Global.asax.cs"] = 2,
                ["CheckoutController.cs"] = 2,
                ["HomeController.cs"] = 2,
                ["AccountController.cs"] = 2,
                ["StoreController.cs"] = 2,
                ["StoreManagerController.cs"] = 2,
                ["AccountModels.cs"] = 2,
                ["Album.cs"] = 2,
                ["MusicStoreEntities.cs"] = 2,
                ["ShoppingCartController.cs"] = 2,
                ["Order.cs"] = 2,
                ["ShoppingCart.cs"] = 2,
            };
            expectedNodeTokenTextChangesCounts.ToList().ForEach(
                expected =>
                {
                    Assert.AreEqual(
                        expected.Value,
                        fileActions.FirstOrDefault(
                            action => action.FilePath.Contains(expected.Key))
                        .NodeTokens.First().TextChanges.Count);
                }
            );
        }

        private void ValidateMvcMusicStore(TestSolutionAnalysis results, string version)
        {
            var analysisResult = results.ProjectResults.FirstOrDefault().ProjectAnalysisResult;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            StringAssert.Contains("HtmlEncoder", analysisResult);
            StringAssert.Contains("EntityFrameworkCore", analysisResult);

            var accountControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "AccountController.cs"));
            var checkoutControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "CheckoutController.cs"));
            var shoppingCartControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "ShoppingCartController.cs"));
            var storeManagerControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "StoreManagerController.cs"));
            var musicStoreEntitiesText = File.ReadAllText(Path.Combine(projectDir, "Models", "MusicStoreEntities.cs"));
            var shoppingCartText = File.ReadAllText(Path.Combine(projectDir, "Models", "ShoppingCart.cs"));


            //Check that namespace was added
            StringAssert.Contains(@"Microsoft.AspNetCore.Mvc", accountControllerText);

            //Check that namespace was added
            StringAssert.Contains(@"Microsoft.AspNetCore.Mvc", checkoutControllerText);

            //Check that method was replaced
            StringAssert.Contains(@"HtmlEncoder.Default.Encode", shoppingCartControllerText);

            //Check that namespaces were added
            StringAssert.Contains(@"Microsoft.AspNetCore.Mvc", storeManagerControllerText);
            StringAssert.Contains(@"Microsoft.EntityFrameworkCore", storeManagerControllerText);

            //Check that namespace was added
            StringAssert.Contains(@"OnConfiguring", musicStoreEntitiesText);
            StringAssert.Contains(@"Microsoft.EntityFrameworkCore", musicStoreEntitiesText);

            //Check that httpcontext is added (with a space) to make sure it's not httpcontextbase
            StringAssert.Contains(@"HttpContext ", shoppingCartText);

            //Check that wwwroot dir was created
            DirectoryAssert.Exists(Path.Combine(projectDir, "wwwroot"));

            //Check that files have been created
            FileAssert.Exists(Path.Combine(projectDir, "Startup.cs"));
            FileAssert.Exists(Path.Combine(projectDir, "Program.cs"));
            FileAssert.Exists(Path.Combine(projectDir, "appsettings.json"));

            //Check that package has been added:
            Assert.True(csProjContent.IndexOf(@"Microsoft.EntityFrameworkCore") > 0);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(@">", version, "<")) > 0);


            Assert.True(csProjContent.IndexOf("Newtonsoft.Json") > 0);


            //Check project actions
            var mvcProjectActions = results.SolutionRunResult.ProjectResults.First(p => p.ProjectFile.EndsWith("MvcMusicStore.csproj"))
                .ExecutedActions.First(a => a.Key == "Project").Value;

            Assert.AreEqual(4, mvcProjectActions.Count);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestAntlrSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("AntlrSample.sln", tempDir, downloadLocation, version);

            // Verify new .csproj file exists
            var addsAntlr3RuntimeProjectFile = results.ProjectResults.First(p => p.CsProjectPath.EndsWith("Adds.Antlr3.Runtime.csproj"));
            FileAssert.Exists(addsAntlr3RuntimeProjectFile.CsProjectPath);

            // No build errors expected in the ported project
            var buildErrors = GetSolutionBuildErrors(results.SolutionRunResult.SolutionPath);
            Assert.AreEqual(0, buildErrors.Count);

            // Verify the new package has been added
            var csProjectContent = addsAntlr3RuntimeProjectFile.CsProjectContent;
            StringAssert.Contains("Include=\"Antlr3.Runtime\"", csProjectContent);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMvcConfigSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("MvcConfigMigrationTest.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            var homeControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "HomeController.cs"));
            ValidateConfig(homeControllerText);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestWebApiConfigSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("WebApiConfigTest.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            var valuesControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "ValuesController.cs"));
            ValidateConfig(valuesControllerText);

        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestBuildableMvcSolution(string version)
        {
            TestSolutionAnalysis resultWithoutCodePort = AnalyzeSolution("BuildableMvc.sln", tempDir, downloadLocation, version, portCode: false);
            var buildErrorsWithoutPortCode = GetSolutionBuildErrors(resultWithoutCodePort.SolutionRunResult.SolutionPath);
            Assert.AreEqual(42, buildErrorsWithoutPortCode.Count);

            TestSolutionAnalysis results = AnalyzeSolution("BuildableMvc.sln", tempDir, downloadLocation, version);
            var buildErrors = GetSolutionBuildErrors(results.SolutionRunResult.SolutionPath);
            Assert.AreEqual(0, buildErrors.Count);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestBuildableWebApiSolution(string version)
        {
            TestSolutionAnalysis resultWithoutCodePort = AnalyzeSolution(
                "BuildableWebApi.sln",
                tempDir,
                downloadLocation,
                version,
                portCode: false);

            var buildErrorsWithoutPortCode = GetSolutionBuildErrors(resultWithoutCodePort.SolutionRunResult.SolutionPath);
            // Build errors were added by 3 because of the PR here:
            // https://github.com/marknfawaz/TestProjects/pull/71
            Assert.AreEqual(38, buildErrorsWithoutPortCode.Count);

            TestSolutionAnalysis results = AnalyzeSolution(
                "BuildableWebApi.sln",
                tempDir,
                downloadLocation,
                version);

            var buildErrors = GetSolutionBuildErrors(results.SolutionRunResult.SolutionPath);
            Assert.AreEqual(0, buildErrors.Count);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestSampleMvcWebApiSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("SampleMvcWebApp.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var homeController = File.ReadAllText(Path.Combine(projectDir, "Controllers", "HomeController.cs"));

            StringAssert.Contains("Microsoft.AspNetCore.Mvc", homeController);
            StringAssert.Contains("This updated method might require the parameters to be re-organized", homeController);
            StringAssert.Contains("TryUpdateModelAsync", homeController);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMemoryUsageForMvcWebApiSolution(string version)
        {
            // This upper limit was chosen by taking the avg memory consumption of this
            // this test over 10 executions, then increasing the limit by ~50%
            var expectedUpperLimitMemoryUsageKb = 30000;

            // Before the execution
            GC.Collect();
            var kbBeforeExecution = GC.GetTotalMemory(false) / 1024;

            // Run analysis and porting
            AnalyzeSolution("SampleMvcWebApp.sln", tempDir, downloadLocation, version);

            // Calculate the memory difference
            var kbAfterExecution = GC.GetTotalMemory(false) / 1024;
            var kbDifference = kbAfterExecution - kbBeforeExecution;

            Assert.LessOrEqual(kbDifference, expectedUpperLimitMemoryUsageKb);
        }

        private void ValidateConfig(string controllerText)
        {
            StringAssert.Contains(@"var conn = ConfigurationManager.Configuration.GetSection(""ConnectionStrings"")[""FirstConnectionString""]", controllerText);
            StringAssert.Contains(@"var conn2 = ConfigurationManager.Configuration.GetSection(""ConnectionStrings"")[""SecondConnectionString""]", controllerText);
            StringAssert.Contains(@"var conn3 = ConfigurationManager.Configuration.GetSection(""ConnectionStrings"")[constConnectionString]", controllerText);

            StringAssert.Contains(@"var webConn1 = ConfigurationManager.Configuration.GetSection(""ConnectionStrings"")[""FirstConnectionString""]", controllerText);
            StringAssert.Contains(@"var webConn2 = ConfigurationManager.Configuration.GetSection(""ConnectionStrings"")[""SecondConnectionString""]", controllerText);
            StringAssert.Contains(@"var webConn3 = ConfigurationManager.Configuration.GetSection(""ConnectionStrings"")[constConnectionString]", controllerText);

            StringAssert.Contains(@"var appSetting = ConfigurationManager.Configuration.GetSection(""appSettings"")[""ClientValidationEnabled""]", controllerText);
            StringAssert.Contains(@"var appSetting3 = ConfigurationManager.Configuration.GetSection(""appSettings"")[constAppSetting]", controllerText);

            StringAssert.DoesNotContain(@"var conn = ConfigurationManager.ConnectionStrings[""FirstConnectionString""];", controllerText);
            StringAssert.DoesNotContain(@"var conn2 = ConfigurationManager.ConnectionStrings[""SecondConnectionString""].ConnectionString;", controllerText);
            StringAssert.DoesNotContain(@"var conn3 = ConfigurationManager.ConnectionStrings[constConnectionString].ConnectionString;", controllerText);

            StringAssert.DoesNotContain(@"var webConn1 = WebConfigurationManager.ConnectionStrings[""FirstConnectionString""];", controllerText);
            StringAssert.DoesNotContain(@"var webConn2 = WebConfigurationManager.ConnectionStrings[""SecondConnectionString""].ConnectionString;", controllerText);
            StringAssert.DoesNotContain(@"var webConn3 = WebConfigurationManager.ConnectionStrings[constConnectionString].ConnectionString;", controllerText);

            StringAssert.DoesNotContain(@"var appSetting = ConfigurationManager.AppSettings[""ClientValidationEnabled""];", controllerText);
            StringAssert.DoesNotContain(@"var appSetting3 = WebConfigurationManager.AppSettings[constAppSetting];", controllerText);
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestIonicZipSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("IonicZipSample.sln", tempDir, downloadLocation, version);

            // Verify new .csproj file exists
            var ionicZipProjectFile = results.ProjectResults.First(p => p.CsProjectPath.EndsWith("IonicZipSample.csproj"));
            FileAssert.Exists(ionicZipProjectFile.CsProjectPath);

            // No build errors expected in the ported project
            var buildErrors = GetSolutionBuildErrors(results.SolutionRunResult.SolutionPath);
            Assert.AreEqual(0, buildErrors.Count);

            // Verify the new package has been added
            var csProjectContent = ionicZipProjectFile.CsProjectContent;
            StringAssert.Contains("Include=\"DotNetZip\"", csProjectContent);
        }
    }
}