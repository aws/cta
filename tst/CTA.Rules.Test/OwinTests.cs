using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Test;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA.Rules.Test
{
    public class OwinTests : AwsRulesBaseTest
    {
        public string tempDir = "";
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
        public async Task TestAspNetRoutes5Solution()
        {
            await TestAspNetRoutes("net5.0");
        }

        [Test]
        public async Task TestAspNetRoutes3Solution()
        {
            await TestAspNetRoutes("netcoreapp3.1");
        }

        private async Task TestAspNetRoutes(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("AspNetRoutes.sln", tempDir, downloadLocation, version);

            var analysisResult = results.SolutionAnalysisResult;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            StringAssert.Contains("Microsoft.AspNetCore.Owin", analysisResult);
            StringAssert.Contains("Replace IOwinContext with HttpContext", analysisResult);

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            //var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var owinapp2Text = File.ReadAllText(Path.Combine(projectDir, "OwinApp2.cs"));

            //Check that namespace was added
            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", owinapp2Text);

            //Check that httpcontext is added (with a space) to make sure it's not httpcontextbase
            StringAssert.Contains(@"HttpContext ", owinapp2Text);

            //Check that namespaces were added
            //StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", programText);
            //StringAssert.Contains(@"Microsoft.Extensions.Hosting;", programText);

            //Check that namespace was added
            StringAssert.Contains(@"Microsoft.AspNetCore.Http", startupText);

            //Check that httpcontext is added (with a space) to make sure it's not httpcontextbase
            StringAssert.Contains(@"HttpContext ", startupText);

            //Check that files have been created
            FileAssert.Exists(Path.Combine(projectDir, "Startup.cs"));
            //FileAssert.Exists(Path.Combine(projectDir, "Program.cs")); // This should be added but class library does not do this

            //Check that package has been added:
            //Assert.True(csProjContent.IndexOf(@"Microsoft.AspNetCore.Owin") > 0);
        }

        [Test]
        public async Task TestBranchingPipelines5Solution()
        {
            await TestBranchingPipelines("net5.0");
        }

        [Test]
        public async Task TestBranchingPipelines3Solution()
        {
            await TestBranchingPipelines("netcoreapp3.1");
        }

        private async Task TestBranchingPipelines(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("BranchingPipelines.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            //var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var displayText = File.ReadAllText(Path.Combine(projectDir, "DisplayBreadCrumbs.cs"));
            var addText = File.ReadAllText(Path.Combine(projectDir, "AddBreadCrumbMiddleware.cs"));

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", displayText);
            StringAssert.Contains(@"HttpContext ", displayText);
            StringAssert.Contains(@"RequestDelegate ", displayText);
            StringAssert.Contains(@"TryGetValue", displayText);
            StringAssert.Contains(@"remove the base class here : OwinMiddleware", displayText);
            //StringAssert.DoesNotContain(@"base(next)", displayText);
            //StringAssert.DoesNotContain(@"override", displayText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", addText);
            StringAssert.Contains(@"HttpContext ", addText);
            StringAssert.Contains(@"RequestDelegate ", addText);
            StringAssert.Contains(@"_next", addText);
            StringAssert.Contains(@"remove the base class here : OwinMiddleware", addText);
            //StringAssert.DoesNotContain(@"base(next)", addText);
            //StringAssert.DoesNotContain(@"override", addText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", startupText);
            StringAssert.Contains(@"HttpContext ", startupText);
            StringAssert.Contains(@"UseMiddleware", startupText);

            //FileAssert.Exists(Path.Combine(projectDir, "Program.cs")); // This should be added but class library does not do this
        }

        [Test]
        public async Task TestEmbedded5Solution()
        {
            await TestEmbedded("net5.0");
        }

        [Test]
        public async Task TestEmbedded3Solution()
        {
            await TestEmbedded("netcoreapp3.1");
        }

        private async Task TestEmbedded(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("Embedded.sln", tempDir, downloadLocation, version);

            var analysisResult = results.SolutionAnalysisResult;

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            StringAssert.Contains("Microsoft.AspNetCore.Owin", analysisResult);
            StringAssert.Contains("Microsoft.AspNetCore.Hosting", analysisResult);
            StringAssert.Contains("Microsoft.AspNetCore.Server.Kestrel", analysisResult);

            //Host program.cs rule and dependencies

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"IApplicationBuilder", startupText);

            StringAssert.Contains("Microsoft.AspNetCore.Hosting", programText);
            StringAssert.Contains("Microsoft.AspNetCore.Server.Kestrel", programText);
            StringAssert.Contains("WebHostBuilder", programText);
        }

        [Test]
        public async Task TestCustomServer5Solution()
        {
            await TestCustomServer("net5.0");
        }

        [Test]
        public async Task TestCustomServer3Solution()
        {
            await TestCustomServer("netcoreapp3.1");
        }

        private async Task TestCustomServer(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("CustomServer.sln", tempDir, downloadLocation, version);

            var myApp = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("MyApp.csproj")).FirstOrDefault();
            FileAssert.Exists(myApp.CsProjectPath);
            var customServer = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("MyCustomServer.csproj")).FirstOrDefault();
            FileAssert.Exists(customServer.CsProjectPath);

            StringAssert.Contains("Microsoft.AspNetCore.Owin", results.SolutionAnalysisResult);
            StringAssert.Contains("Microsoft.AspNetCore.Hosting", results.SolutionAnalysisResult);
            StringAssert.Contains("Microsoft.AspNetCore.Server.Kestrel", results.SolutionAnalysisResult);

            //MyApp
            var startupText = File.ReadAllText(Path.Combine(myApp.ProjectDirectory, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(myApp.ProjectDirectory, "Program.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"IApplicationBuilder", startupText);

            StringAssert.Contains("WebHostBuilder", programText);

            //Check for comment on how to implement a custom server here in program

            //CustomServer
            var customerServerText = File.ReadAllText(Path.Combine(customServer.ProjectDirectory, "CustomServer.cs"));
            var serverFactoryText = File.ReadAllText(Path.Combine(customServer.ProjectDirectory, "ServerFactory.cs"));

            //MyCustomerServer very difficult
            //Keep their server intact but must implement IServer instead of just IDisposable
            //Change their Start class to implement StartAsync instead and change reference to it to startAsync also
        }

        [Test]
        public async Task TestHelloWorld5Solution()
        {
            await TestHelloWorld("net5.0");
        }

        [Test]
        public async Task TestHelloWorld3Solution()
        {
            await TestHelloWorld("netcoreapp3.1");
        }

        private async Task TestHelloWorld(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("HelloWorld.sln", tempDir, downloadLocation, version);

            var analysisResult = results.SolutionAnalysisResult;
            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;

            StringAssert.Contains("Microsoft.AspNetCore.Owin", analysisResult);
            StringAssert.Contains("Replace IOwinContext with HttpContext", analysisResult);

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            //Check that namespace was added
            StringAssert.Contains(@"Microsoft.AspNetCore.Http", startupText);

            //Check that httpcontext is added (with a space) to make sure it's not httpcontextbase
            StringAssert.Contains(@"HttpContext ", startupText);
        }

        [Test]
        public async Task TestHelloWorldRawOwin5Solution()
        {
            await TestHelloWorldRawOwin("net5.0");
        }

        [Test]
        public async Task TestHelloWorldRawOwin3Solution()
        {
            await TestHelloWorldRawOwin("netcoreapp3.1");
        }

        private async Task TestHelloWorldRawOwin(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("HelloWorldRawOwin.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"IApplicationBuilder", startupText);
            StringAssert.Contains(@"UseOwin", startupText);
        }

        [Test]
        public async Task TestOwinSelfHostSample5Solution()
        {
            await TestOwinSelfHostSample("net5.0");
        }

        [Test]
        public async Task TestOwinSelfHostSample3Solution()
        {
            await TestOwinSelfHostSample("netcoreapp3.1");
        }

        private async Task TestOwinSelfHostSample(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("OwinSelfhostSample.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"UseEndpoints", startupText);
            StringAssert.Contains(@"ConfigureServices", startupText);
            StringAssert.Contains(@"MapControllers", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);
            StringAssert.DoesNotContain(@"System.Web.Http", startupText);

            StringAssert.Contains("WebHostBuilder", programText);
        }

        [Test]
        public async Task TestSignalR5Solution()
        {
            await TestSignalR("net5.0");
        }

        [Test]
        public async Task TestSignalR3Solution()
        {
            await TestSignalR("netcoreapp3.1");
        }

        private async Task TestSignalR(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("SignalR.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"UseSignalR", startupText);
            StringAssert.Contains(@"ConfigureServices", startupText);
            StringAssert.Contains(@"AddSignalR", startupText);
            StringAssert.Contains(@"MapHub", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);

            StringAssert.Contains("WebHostBuilder", programText);
        }

        [Test]
        public async Task TestStaticFilesSample5Solution()
        {
            await TestStaticFilesSample("net5.0");
        }

        [Test]
        public async Task TestStaticFilesSample3Solution()
        {
            await TestStaticFilesSample("netcoreapp3.1");
        }

        private async Task TestStaticFilesSample(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("StaticFilesSample.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"UseDeveloperExceptionPage", startupText);
            StringAssert.Contains(@"AddDirectoryBrowser", startupText);
            StringAssert.Contains(@"FileProvider", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.FileProviders", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.StaticFiles", startupText);
            StringAssert.DoesNotContain(@"Microsoft.Owin.StaticFiles", startupText);
            StringAssert.DoesNotContain(@"Microsoft.Owin.FileSystems", startupText);

            StringAssert.Contains("WebHostBuilder", programText);
        }

        [Test]
        public async Task TestWebApi5Solution()
        {
            await TestWebApi("net5.0");
        }

        [Test]
        public async Task TestWebApi3Solution()
        {
            await TestWebApi("netcoreapp3.1");
        }

        private async Task TestWebApi(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("OwinWebApi.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            StringAssert.Contains(@"UseEndpoints", startupText);
            StringAssert.Contains(@"ConfigureServices", startupText);
            StringAssert.Contains(@"MapControllers", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);
        }

        [Test]
        public async Task TestWebSocketSample5Solution()
        {
            await TestWebSocketSample("net5.0");
        }

        [Test]
        public async Task TestWebSocketSample3Solution()
        {
            await TestWebSocketSample("netcoreapp3.1");
        }

        private async Task TestWebSocketSample(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("WebSocketSample.sln", tempDir, downloadLocation, version);

            var sampleClient = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("SampleClient.csproj")).FirstOrDefault();
            FileAssert.Exists(sampleClient.CsProjectPath);
            var webSocketServer = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("WebSocketServer.csproj")).FirstOrDefault();
            FileAssert.Exists(webSocketServer.CsProjectPath);

            var clientProgramText = File.ReadAllText(Path.Combine(sampleClient.ProjectDirectory, "Program.cs"));

            var serverStartupText = File.ReadAllText(Path.Combine(webSocketServer.ProjectDirectory, "Startup.cs"));
            var serverProgramText = File.ReadAllText(Path.Combine(webSocketServer.ProjectDirectory, "Program.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", serverStartupText);
            StringAssert.Contains(@"IApplicationBuilder", serverStartupText);
            StringAssert.Contains(@"UseOwin", serverStartupText);

            StringAssert.Contains("WebHostBuilder", serverProgramText);
        }

        [TearDown]
        public void Cleanup()
        {
            Directory.Delete(GetTstPath(Path.Combine(new string[] { "Projects", "Temp" })), true);
        }
    }
}
