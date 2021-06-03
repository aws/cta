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
            tempDir = GetTstPath(Path.Combine(new string[] { "Projects", "Temp", "Owin" }));
            DownloadTestProjects();
        }

        private void DownloadTestProjects()
        {
            downloadLocation = DownloadTestProjects(tempDir);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestAspNetRoutes(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("AspNetRoutes.sln", tempDir, downloadLocation, version);

            var analysisResult = results.SolutionAnalysisResult;
            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;

            StringAssert.Contains("Microsoft.AspNetCore.Owin", analysisResult);
            StringAssert.Contains("Replace IOwinContext with HttpContext", analysisResult);

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            //var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var owinapp2Text = File.ReadAllText(Path.Combine(projectDir, "OwinApp2.cs"));

            //Check that namespaces were added
            //StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", programText);
            //StringAssert.Contains(@"Microsoft.Extensions.Hosting;", programText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", owinapp2Text);
            StringAssert.Contains(@"HttpContext ", owinapp2Text);
            StringAssert.Contains(@"IQueryCollection ", owinapp2Text);
            StringAssert.Contains(@"IRequestCookieCollection ", owinapp2Text);
            StringAssert.DoesNotContain(@"IReadableStringCollection ", owinapp2Text);

            StringAssert.Contains(@"Microsoft.AspNetCore.Http", startupText);
            StringAssert.Contains(@"HttpContext ", startupText);
            StringAssert.Contains(@"IResponseCookies ", startupText);
            StringAssert.DoesNotContain(@"ResponseCookieCollection ", startupText);

            FileAssert.Exists(Path.Combine(projectDir, "Startup.cs"));
            //FileAssert.Exists(Path.Combine(projectDir, "Program.cs")); // This should be added but class library does not do this

            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestBranchingPipelines(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("BranchingPipelines.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            //var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var displayText = File.ReadAllText(Path.Combine(projectDir, "DisplayBreadCrumbs.cs"));
            var addText = File.ReadAllText(Path.Combine(projectDir, "AddBreadCrumbMiddleware.cs"));

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", displayText);
            StringAssert.Contains(@"HttpContext ", displayText);
            StringAssert.Contains(@"HttpRequest ", displayText);
            StringAssert.Contains(@"HttpResponse ", displayText);
            StringAssert.Contains(@"RequestDelegate ", displayText);
            StringAssert.Contains(@"TryGetValue", displayText);
            StringAssert.Contains(@"RequestDelegate _next", displayText);
            StringAssert.Contains(@"_next = next;", displayText);
            StringAssert.DoesNotContain(@"base(next)", displayText);
            StringAssert.DoesNotContain(@": OwinMiddleware", displayText);
            StringAssert.DoesNotContain(@"override", displayText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", addText);
            StringAssert.Contains(@"HttpContext ", addText);
            StringAssert.Contains(@"HttpRequest ", addText);
            StringAssert.Contains(@"RequestDelegate ", addText);
            StringAssert.Contains(@"_next", addText);
            StringAssert.Contains(@"RequestDelegate _next", addText);
            StringAssert.Contains(@"_next = next;", addText);
            StringAssert.Contains(@"_next.Invoke", addText);
            StringAssert.DoesNotContain(@"base(next)", addText);
            StringAssert.DoesNotContain(@": OwinMiddleware", addText);
            StringAssert.DoesNotContain(@"override", addText);

            StringAssert.Contains(@"using Microsoft.AspNetCore.Http", startupText);
            StringAssert.Contains(@"HttpContext ", startupText);
            StringAssert.Contains(@"Please replace CreatePerOwinContext<T>(System.Func<T>)", startupText);
            StringAssert.Contains(@"Please replace CreatePerOwinContext<T>(System.Func<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, Microsoft.Owin.IOwinContext, T>)", startupText);
            StringAssert.Contains(@"Please replace CreatePerOwinContext<T>(System.Func<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, Microsoft.Owin.IOwinContext, T>, System.Action<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, T>)", startupText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);

            //FileAssert.Exists(Path.Combine(projectDir, "Program.cs")); // This should be added but class library does not do this
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestEmbedded(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("Embedded.sln", tempDir, downloadLocation, version);

            var analysisResult = results.SolutionAnalysisResult;

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;

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

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestCustomServer(string version)
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
            var myappProjContent = myApp.CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(myApp.ProjectDirectory, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(myApp.ProjectDirectory, "Program.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"IApplicationBuilder", startupText);

            StringAssert.Contains("WebHostBuilder", programText);

            //Check for comment on how to implement a custom server here in program

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", myappProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", myappProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", myappProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", myappProjContent);

            //Check that correct version is used
            Assert.True(myappProjContent.IndexOf(string.Concat(">", version, "<")) > 0);

            //CustomServer
            var customProjContent = customServer.CsProjectContent;
            var customerServerText = File.ReadAllText(Path.Combine(customServer.ProjectDirectory, "CustomServer.cs"));
            var serverFactoryText = File.ReadAllText(Path.Combine(customServer.ProjectDirectory, "ServerFactory.cs"));

            //MyCustomerServer very difficult
            //Keep their server intact but must implement IServer instead of just IDisposable
            //Change their Start class to implement StartAsync instead and change reference to it to startAsync also

            //Check that correct version is used
            Assert.True(customProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }


        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestHelloWorld(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("HelloWorld.sln", tempDir, downloadLocation, version);

            var analysisResult = results.SolutionAnalysisResult;
            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;

            StringAssert.Contains("Microsoft.AspNetCore.Owin", analysisResult);
            StringAssert.Contains("Replace IOwinContext with HttpContext", analysisResult);

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            //Check that namespace was added
            StringAssert.Contains(@"Microsoft.AspNetCore.Http", startupText);

            //Check that httpcontext is added (with a space) to make sure it's not httpcontextbase
            StringAssert.Contains(@"HttpContext ", startupText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestHelloWorldRawOwin(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("HelloWorldRawOwin.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"IApplicationBuilder", startupText);
            StringAssert.Contains(@"UseOwin", startupText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestOwinSelfHostSample(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("OwinSelfhostSample.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"UseEndpoints", startupText);
            StringAssert.Contains(@"ConfigureServices", startupText);
            StringAssert.Contains(@"MapControllers", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);
            StringAssert.DoesNotContain(@"System.Web.Http", startupText);

            StringAssert.Contains("WebHostBuilder", programText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestSignalR(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("SignalR.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"UseSignalR", startupText);
            StringAssert.Contains(@"ConfigureServices", startupText);
            StringAssert.Contains(@"AddSignalR", startupText);
            StringAssert.Contains(@"MapHub", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);

            StringAssert.Contains("WebHostBuilder", programText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.SignalR", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestStaticFilesSample(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("StaticFilesSample.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.Contains(@"UseDeveloperExceptionPage", startupText);
            StringAssert.Contains(@"AddDirectoryBrowser", startupText);
            StringAssert.Contains(@"FileProvider", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.FileProviders", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.StaticFiles", startupText);
            StringAssert.Contains(@"PhysicalFileProvider", startupText);
            StringAssert.Contains(@"FileProvider", startupText);
            StringAssert.Contains(@"For FileServerOptions, if FileSystem was not present before FileProvider was added", startupText);
            StringAssert.Contains(@"For DirectoryBrowserOptions, if FileSystem was not present before FileProvider was added", startupText);
            StringAssert.DoesNotContain(@"PhysicalFileSystems", startupText);
            StringAssert.DoesNotContain(@"Microsoft.Owin.StaticFiles", startupText);
            StringAssert.DoesNotContain(@"Microsoft.Owin.FileSystems", startupText);
 
            StringAssert.Contains("WebHostBuilder", programText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.StaticFiles", csProjContent);
            StringAssert.Contains(@"Microsoft.Extensions.FileProviders.Physical", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestWebApi(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("OwinWebApi.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            StringAssert.Contains(@"UseEndpoints", startupText);
            StringAssert.Contains(@"ConfigureServices", startupText);
            StringAssert.Contains(@"MapControllers", startupText);
            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", startupText);
            StringAssert.Contains(@"Microsoft.Extensions.DependencyInjection", startupText);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", csProjContent);

            //Check that correct version is used
            Assert.True(csProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestWebSocketSample(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("WebSocketSample.sln", tempDir, downloadLocation, version);

            var sampleClient = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("SampleClient.csproj")).FirstOrDefault();
            FileAssert.Exists(sampleClient.CsProjectPath);
            var webSocketServer = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("WebSocketServer.csproj")).FirstOrDefault();
            FileAssert.Exists(webSocketServer.CsProjectPath);

            var clientProjContent = sampleClient.CsProjectContent;
            var clientProgramText = File.ReadAllText(Path.Combine(sampleClient.ProjectDirectory, "Program.cs"));

            var serverProjContent = webSocketServer.CsProjectContent;
            var serverStartupText = File.ReadAllText(Path.Combine(webSocketServer.ProjectDirectory, "Startup.cs"));
            var serverProgramText = File.ReadAllText(Path.Combine(webSocketServer.ProjectDirectory, "Program.cs"));

            StringAssert.Contains(@"Microsoft.AspNetCore.Builder", serverStartupText);
            StringAssert.Contains(@"IApplicationBuilder", serverStartupText);
            StringAssert.Contains(@"UseOwin", serverStartupText);

            StringAssert.Contains("WebHostBuilder", serverProgramText);

            //Check that correct version is used
            Assert.True(clientProjContent.IndexOf(string.Concat(">", version, "<")) > 0);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", serverProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", serverProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", serverProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", serverProjContent);

            Assert.True(serverProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TearDown]
        public void Cleanup()
        {
            Directory.Delete(tempDir, true);
        }
    }
}
