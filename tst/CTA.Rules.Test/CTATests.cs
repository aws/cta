using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.RuleFiles;
using CTA.Rules.Update;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CTA.Rules.Test
{
    public class CTATests : AwsRulesBaseTest
    {
        public string tempDir = "";
        public string downloadLocation;
        public List<string> ctaFiles;
        public string version = "net5.0"; //We don't care about version for CTA-only rules:

        [SetUp]
        public void Setup()
        {
            tempDir = SetupTests.TempDir;
            downloadLocation = SetupTests.DownloadLocation;
            ctaFiles = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CTAFiles")), "*.json")
               .Select(s => Path.GetFileNameWithoutExtension(s))
               .ToList();
        }

        [Test]
        public void TestCli()
        {
            string mvcMusicStorePath = Directory.EnumerateFiles(tempDir, "MvcMusicStore.sln", SearchOption.AllDirectories).FirstOrDefault();
            string mvcMusicStoreProjectPath = Directory.EnumerateFiles(tempDir, "MvcMusicStore.csproj", SearchOption.AllDirectories).FirstOrDefault();
            string[] args = { "-p", mvcMusicStoreProjectPath, "-s", mvcMusicStorePath, "-r", Path.Combine(tempDir, "Input"), "-a", "", "-m", "true" };
            var cli = new RulesCli();
            cli.HandleCommand(args);
            Assert.NotNull(cli);
            Assert.NotNull(cli.Project);
            Assert.NotNull(cli.FilePath);
            Assert.NotNull(cli.AssembliesDir);
            Assert.NotNull(cli.IsMockRun);

        }

        private TestSolutionAnalysis runCTAFile(string solutionName, string projectName = null) 
        {
            var solutionPath = CopySolutionFolderToTemp(solutionName, downloadLocation);
            var solutionDir = Directory.GetParent(solutionPath).FullName;

            FileAssert.Exists(solutionPath);

            //Sample Web API has only one project:
            string projectFile = (projectName == null ? Utils.GetProjectPaths(Path.Combine(solutionDir, solutionName)).FirstOrDefault() : Directory.EnumerateFiles(solutionDir, projectName, SearchOption.AllDirectories).FirstOrDefault());
            FileAssert.Exists(projectFile);

            ProjectConfiguration projectConfiguration = new ProjectConfiguration()
            {
                SolutionPath = solutionPath,
                ProjectPath = projectFile,
                TargetVersions = new List<string> { version },
                RulesDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CTAFiles")),
                AdditionalReferences = ctaFiles
            };

            List<ProjectConfiguration> solutionConfiguration = new List<ProjectConfiguration>
            {
                projectConfiguration
            };

            SolutionRewriter solutionRewriter = new SolutionRewriter(solutionPath, solutionConfiguration);
            var analysisRunResult = solutionRewriter.AnalysisRun();
            StringBuilder str = new StringBuilder();
            foreach (var projectResult in analysisRunResult.ProjectResults)
            {
                str.AppendLine(projectResult.ProjectFile);
                str.AppendLine(projectResult.ProjectActions.ToString());
            }
            var analysisResult = str.ToString();
            solutionRewriter.Run(analysisRunResult.ProjectResults.ToDictionary(p => p.ProjectFile, p => p.ProjectActions));

            TestSolutionAnalysis result = new TestSolutionAnalysis()
            {
                SolutionAnalysisResult = analysisResult,
                ProjectResults = new List<ProjectResult>()
                {
                    new ProjectResult()
                    {
                        ProjectAnalysisResult = analysisResult,
                        CsProjectPath = projectFile,
                        ProjectDirectory = Directory.GetParent(projectFile).FullName,
                        CsProjectContent = File.ReadAllText(projectFile)
        }
                }
            };

            return result;
        }

        [Test]
        public void TestSampleWebApiSolution()
        {
            var results = runCTAFile("WebApiWithReferences.sln", "WebApiWithReferences.csproj").ProjectResults.FirstOrDefault();
            StringAssert.Contains("IActionResult", results.ProjectAnalysisResult);

            var homeControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "HouseController.cs"));
            var iHouseRepositoryText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Repositories", "IHouseRepository.cs"));
            //Check that namespace has been added
            StringAssert.Contains(@"Microsoft.AspNetCore.Mvc", homeControllerText);

            //Check that attribute has been added to class, and base class changed:
            StringAssert.Contains(@"[ApiController]", homeControllerText);
            StringAssert.Contains(@"ControllerBase", homeControllerText);

            //Check that invocation expression was replaced:
            StringAssert.Contains(@"this.Response.Headers.Add", homeControllerText);

            //Check that identifier as replaced:
            StringAssert.Contains(@"IActionResult", homeControllerText);

            //Interface Tests
            StringAssert.Contains(@"AnyMethod", iHouseRepositoryText);
            StringAssert.Contains(@"using System;", iHouseRepositoryText);
            StringAssert.Contains(@"[Authorize]", iHouseRepositoryText);
            StringAssert.DoesNotContain(@"Delete", iHouseRepositoryText);
            StringAssert.Contains(@"IChangedHouseRepository", iHouseRepositoryText);
            StringAssert.Contains(@"This is a commented interface", iHouseRepositoryText);
            StringAssert.DoesNotContain(@"[Description(", iHouseRepositoryText);

        }

        [Test]
        public void TestMvcMusicStore()
        {
            var results = runCTAFile("MvcMusicStore.sln").ProjectResults.FirstOrDefault();

            StringAssert.Contains("HtmlEncoder", results.ProjectAnalysisResult);

            var accountControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "AccountController.cs"));
            var checkoutControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "CheckoutController.cs"));
            var shoppingCartControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "ShoppingCartController.cs"));
            var storeManagerControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "StoreManagerController.cs"));
            var musicStoreEntitiesText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Models", "MusicStoreEntities.cs"));
            var shoppingCartText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Models", "ShoppingCart.cs"));

            var shoppingCartRemoveViewModel = File.ReadAllText(Path.Combine(results.ProjectDirectory, "ViewModels", "ShoppingCartRemoveViewModel.cs"));
            var shoppingCartViewModel = File.ReadAllText(Path.Combine(results.ProjectDirectory, "ViewModels", "ShoppingCartViewModel.cs"));


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

            //Test change attribute name
            StringAssert.Contains(@"ChangedChildActionOnly", shoppingCartControllerText);

            //Test change namespace name
            StringAssert.Contains(@"MvcMusicStore.ChangedViewModel", shoppingCartRemoveViewModel);
            StringAssert.Contains(@"MvcMusicStore.ChangedViewModel", shoppingCartViewModel);

            StringAssert.Contains(@"using System;", shoppingCartControllerText);
            StringAssert.Contains(@"int PickRandomNumberRange", shoppingCartControllerText);
            StringAssert.Contains(@"Something.Something()", shoppingCartControllerText);
            StringAssert.DoesNotContain(@"new MusicStoreEntities()", shoppingCartControllerText);
            StringAssert.Contains(@"SomethingElse storeDB", shoppingCartControllerText);
        }


        [Test]
        public void TestMonolithReplacementsMVC()
         {
            var results = runCTAFile("MvcMusicStore.sln").ProjectResults.FirstOrDefault();

            var storeManagerControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "StoreManagerController.cs"));

            FileAssert.Exists(Path.Combine(results.ProjectDirectory, Constants.MonolithServiceMvc + ".cs"));
            StringAssert.Contains(@"return Content(MonolithService.CreateRequest(Request, this.ControllerContext.RouteData));", storeManagerControllerText);
        }

        [Test]
        public void TestMonolithReplacementsWebAPI()
        {
            var results = runCTAFile("SampleWebApi.sln").ProjectResults.FirstOrDefault();
            var houseControllerText = File.ReadAllText(Path.Combine(results.ProjectDirectory, "Controllers", "HouseController.cs"));

            FileAssert.Exists(Path.Combine(results.ProjectDirectory, Constants.MonolithService + ".cs"));
            StringAssert.Contains(@"return Content(MonolithService.CreateRequest(Request, this.ControllerContext.RouteData));", houseControllerText);
        }

        [Test]
        public void TestProjectsOutsideSolutionPath_Are_Ported()
        {
            var results = AnalyzeSolution("Application_Proj_diff_folder.sln", tempDir, downloadLocation, version);

            FileAssert.Exists(results.ProjectResults[0].CsProjectPath);
            FileAssert.Exists(results.ProjectResults[1].CsProjectPath);

            var csProjContent1 = results.ProjectResults[0].CsProjectContent;
            var csProjContent2 = results.ProjectResults[1].CsProjectContent;

            StringAssert.Contains(string.Concat(">", version, "<"), csProjContent1);
            StringAssert.Contains(string.Concat(">", version, "<"), csProjContent2);
        }

        [Test]
        public void ConvertHierarchicalToNamespaceFile()
        {
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CTAFiles");
            var consolidatedFile = Directory.EnumerateFiles(dir, "consolidated.json").FirstOrDefault();
            FileAssert.Exists(consolidatedFile);

            RulesFileExport rulesFileExport = new RulesFileExport(consolidatedFile);
            rulesFileExport.Run();

            FileAssert.Exists(Path.Combine(dir, "microsoft.sqlserver.types.json"));
            FileAssert.Exists(Path.Combine(dir, "system.data.entity.json"));
            FileAssert.Exists(Path.Combine(dir, "system.data.sqlclient.json"));
            FileAssert.Exists(Path.Combine(dir, "system.runtime.caching.json"));
            FileAssert.Exists(Path.Combine(dir, "system.web.http.json"));
            FileAssert.Exists(Path.Combine(dir, "system.web.http.odata.json"));
            FileAssert.Exists(Path.Combine(dir, "system.web.json"));
            FileAssert.Exists(Path.Combine(dir, "system.web.mvc.json"));
        }

        [Test]
        public void LoggerTest()
        {
            LogHelper.LogError(new Exception("Test message 1"));
            LogHelper.LogError("Test message 1");
            LogHelper.LogDebug("Test message 1");
            LogHelper.LogWarning("Test message 1");
            LogHelper.LogInformation("Test message 1");
            LogHelper.Logger = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug)).CreateLogger(Constants.Translator);
        }
    }
}