using CTA.Rules.PortCore;
using NUnit.Framework;
using System.IO;
using System.Linq;

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
        public void TestSampleWebApi5Solution()
        {
            TestWebApi("net5.0");
        }

        [Test]
        public void TestSampleWebApi3Solution()
        {
            TestWebApi("netcoreapp3.1");
            SolutionPort.ResetCache(false, false);
            TestWebApi("net5.0", Path.Combine(tempDir, "netcoreapp3.1"));
        }

        private void TestWebApi(string version, string solutionDir = "")
        {
            TestSolutionAnalysis results;

            if (string.IsNullOrEmpty(solutionDir))
            {
                results = AnalyzeSolution("SampleWebApi.sln", tempDir, downloadLocation, version);
            }else
            {
                results = AnalyzeSolution("SampleWebApi.sln", solutionDir, downloadLocation, version, true);
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

        [Test]
        public void TestSampleWebApiWithReferences3Solution()
        {
            TestWebApiWithReferences("netcoreapp3.1");
        }

        private void TestWebApiWithReferences(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("WebApiWithReferences.sln", tempDir, downloadLocation, version);

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

        [Test]
        public void TestMvcMusicStore5()
        {
            TestMvcMusicStore("net5.0");
        }

        [Test]
        public void TestMvcMusicStore3()
        { 
            TestMvcMusicStore("netcoreapp3.1");
        }

        private void TestMvcMusicStore(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("MvcMusicStore.sln", tempDir, downloadLocation, version);

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

        [TearDown]
        public void Cleanup()
        {
            Directory.Delete(GetTstPath(Path.Combine(new string[] { "Projects", "Temp" })), true);
        }
    }
}