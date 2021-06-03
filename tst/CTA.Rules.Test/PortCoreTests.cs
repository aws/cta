using CTA.Rules.PortCore;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CTA.Rules.Test
{
    public class PortCoreTests : AwsRulesBaseTest
    {
        public string tempDir;
        public string downloadLocation;

        [SetUp]
        public void Setup()
        {
            Setup(this.GetType());
            tempDir = GetTstPath(Path.Combine(new string[] { "Projects", "Temp" }));
            DownloadTestProjects();
        }

        private void DownloadTestProjects()
        {
            downloadLocation = DownloadTestProjects(tempDir);
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
            TestWebApi(TargetFramework.DotnetCoreApp31);
            SolutionPort.ResetCache(false, false);
            TestWebApi(TargetFramework.Dotnet5, Path.Combine(tempDir, TargetFramework.DotnetCoreApp31));
        }

        [TestCase(TargetFramework.Dotnet5)]
        public void TestWebApi(string version, string solutionDir = "")
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
                Assert.AreEqual(webApiProjectActions.Count, 4);
            }

            //When running the second time
            else
            {
                Assert.AreEqual(webApiProjectActions.Count, 2);
            }
        }

        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestWebApiWithReferences(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("WebApiWithReferences.sln", tempDir);
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version);
            
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

            Assert.AreEqual(classlibrary1Actions.Count, 2);
            Assert.AreEqual(classlibrary2Actions.Count, 2);
            Assert.AreEqual(webApiProjectActions.Count, 4);
        }
        
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMvcMusicStore(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("MvcMusicStore.sln", tempDir);
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version);

            ValidateMvcMusicStore(results, version);            
        }        
        
  


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

            Assert.AreEqual(mvcProjectActions.Count, 4);
        }

        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestAntlrSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("AntlrSample.sln", tempDir, downloadLocation, version);

            // Verify new .csproj file exists
            var addsAntlr3RuntimeProjectFile = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("Adds.Antlr3.Runtime.csproj")).FirstOrDefault();
            FileAssert.Exists(addsAntlr3RuntimeProjectFile.CsProjectPath);

            // No build errors expected in the ported project
            Assert.False(results.SolutionRunResult.BuildErrors[addsAntlr3RuntimeProjectFile.CsProjectPath].Any());

            // Verify the new package has been added
            var csProjectContent = addsAntlr3RuntimeProjectFile.CsProjectContent;
            StringAssert.Contains("Include=\"Antlr3.Runtime\"", csProjectContent);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestMvcConfigSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("MvcConfigMigrationTest.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            var homeControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "HomeController.cs"));
            ValidateConfig(homeControllerText);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestWebApiConfigSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("WebApiConfigTest.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            var valuesControllerText = File.ReadAllText(Path.Combine(projectDir, "Controllers", "ValuesController.cs"));
            ValidateConfig(valuesControllerText);

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

        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestIonicZipSampleSolution(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("IonicZipSample.sln", tempDir, downloadLocation, version);

            // Verify new .csproj file exists
            var ionicZipProjectFile = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("IonicZipSample.csproj")).FirstOrDefault();
            FileAssert.Exists(ionicZipProjectFile.CsProjectPath);

            // No build errors expected in the ported project
            Assert.False(results.SolutionRunResult.BuildErrors[ionicZipProjectFile.CsProjectPath].Any());

            // Verify the new package has been added
            var csProjectContent = ionicZipProjectFile.CsProjectContent;
            StringAssert.Contains("Include=\"DotNetZip\"", csProjectContent);
        }

        [TearDown]
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
                    Directory.Delete(GetTstPath(Path.Combine("Projects", "Temp")), true);
                }
                catch (Exception)
                {
                    Thread.Sleep(10000);
                    DeleteDir(retries + 1);
                }
            }
        }
    }
}