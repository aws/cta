using NUnit.Framework;
using System.IO;
using System.Linq;

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
        public void TestAspNetRoutes5Solution()
        {
            TestAspNetRoutes("net5.0");
        }

        [Test]
        public void TestAspNetRoutes3Solution()
        {
            TestAspNetRoutes("netcoreapp3.1");
        }

        private void TestAspNetRoutes(string version)
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
        public void TestBranchingPipelines5Solution()
        {
            TestBranchingPipelines("net5.0");
        }

        [Test]
        public void TestBranchingPipelines3Solution()
        {
            TestBranchingPipelines("netcoreapp3.1");
        }

        private void TestBranchingPipelines(string version)
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
            StringAssert.DoesNotContain(@": OwinMiddleware", displayText);
            //StringAssert.DoesNotContain(@"base(next)", displayText);
            StringAssert.DoesNotContain(@"override", displayText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", addText);
            StringAssert.Contains(@"HttpContext ", addText);
            StringAssert.Contains(@"RequestDelegate ", addText);
            StringAssert.Contains(@"_next", addText);
            StringAssert.DoesNotContain(@": OwinMiddleware", addText);
            //StringAssert.DoesNotContain(@"base(next)", addText);
            StringAssert.DoesNotContain(@"override", addText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", startupText);
            StringAssert.Contains(@"HttpContext ", startupText);
            StringAssert.Contains(@"UseMiddleware", startupText);

            //FileAssert.Exists(Path.Combine(projectDir, "Program.cs")); // This should be added but class library does not do this
        }

        [Test]
        public void TestEmbedded5Solution()
        {
            TestEmbedded("net5.0");
        }

        [Test]
        public void TestEmbedded3Solution()
        {
            TestEmbedded("netcoreapp3.1");
        }

        private void TestEmbedded(string version)
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
        public void TestCustomServer5Solution()
        {
            TestCustomServer("net5.0");
        }

        [Test]
        public void TestCustomServer3Solution()
        {
            TestCustomServer("netcoreapp3.1");
        }

        private void TestCustomServer(string version)
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
        public void TestHelloWorld5Solution()
        {
            TestHelloWorld("net5.0");
        }

        [Test]
        public void TestHelloWorld3Solution()
        {
            TestHelloWorld("netcoreapp3.1");
        }

        private void TestHelloWorld(string version)
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
        public void TestHelloWorldRawOwin5Solution()
        {
            TestHelloWorldRawOwin("net5.0");
        }

        [Test]
        public void TestHelloWorldRawOwin3Solution()
        {
            TestHelloWorldRawOwin("netcoreapp3.1");
        }

        private void TestHelloWorldRawOwin(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("HelloWorldRawOwin.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"IApplicationBuilder", startupText);
            StringAssert.Contains(@"UseOwin", startupText);
        }

        [Test]
        public void TestOwinSelfHostSample5Solution()
        {
            TestOwinSelfHostSample("net5.0");
        }

        [Test]
        public void TestOwinSelfHostSample3Solution()
        {
            TestOwinSelfHostSample("netcoreapp3.1");
        }

        private void TestOwinSelfHostSample(string version)
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
        public void TestSignalR5Solution()
        {
            TestSignalR("net5.0");
        }

        [Test]
        public void TestSignalR3Solution()
        {
            TestSignalR("netcoreapp3.1");
        }

        private void TestSignalR(string version)
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
        public void TestStaticFilesSample5Solution()
        {
            TestStaticFilesSample("net5.0");
        }

        [Test]
        public void TestStaticFilesSample3Solution()
        {
            TestStaticFilesSample("netcoreapp3.1");
        }

        private void TestStaticFilesSample(string version)
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
        public void TestWebApi5Solution()
        {
            TestWebApi("net5.0");
        }

        [Test]
        public void TestWebApi3Solution()
        {
            TestWebApi("netcoreapp3.1");
        }

        private void TestWebApi(string version)
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
        public void TestWebSocketSample5Solution()
        {
            TestWebSocketSample("net5.0");
        }

        [Test]
        public void TestWebSocketSample3Solution()
        {
            TestWebSocketSample("netcoreapp3.1");
        }

        private void TestWebSocketSample(string version)
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
