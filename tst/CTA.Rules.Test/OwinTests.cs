using CTA.Rules.Test.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CTA.Rules.Test
{
    public class OwinTests : AwsRulesBaseTest
    {
        public string tempDir = "";
        public string downloadLocation;

        [SetUp]
        public void Setup()
        {
            tempDir = SetupTests.TempDir;
            downloadLocation = SetupTests.DownloadLocation;
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestAspNetRoutes(string version)
        {
             TestSolutionAnalysis results = AnalyzeSolution("AspNetRoutes.sln", tempDir, downloadLocation, version);

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            //var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var owinapp2Text = File.ReadAllText(Path.Combine(projectDir, "OwinApp2.cs"));

            //Check that namespaces were added
            //StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", programText);
            //StringAssert.Contains(@"Microsoft.Extensions.Hosting;", programText);

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.AspNetRoutesOwinApp2, owinapp2Text);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.AspNetRoutesStartup, startupText);

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

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.BranchingPipelinesDisplayBreadCrumbs, displayText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.BranchingPipelinesAddBreadCrumbMiddleware, addText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.BranchingPipelinesStartup, startupText);

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

            string projectDir = results.ProjectResults.FirstOrDefault().ProjectDirectory;
            var csProjContent = results.ProjectResults.FirstOrDefault().CsProjectContent;

            //Host program.cs rule and dependencies
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.EmbeddedStartup, startupText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.EmbeddedProgram, programText);

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

            //MyApp
            var myappProjContent = myApp.CsProjectContent;
            var startupText = File.ReadAllText(Path.Combine(myApp.ProjectDirectory, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(myApp.ProjectDirectory, "Program.cs"));

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.CustomServerProgram, programText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.CustomServerStartup, startupText);

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

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.HelloWorldStartup, startupText);

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

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.HelloWorldRawOwinStartup, startupText);

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

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.OwinSelfHostProgram, programText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.OwinSelfHostStartup, startupText);

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

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.SignalRProgram, programText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.SignalRStartup, startupText);

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

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.StaticFilesSampleProgram, programText);
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.StaticFilesSampleStartup, startupText);

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

            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.OwinWebApiStartup, startupText);

            //Check that package has been added:
            StringAssert.Contains(version == TargetFramework.Dotnet5 ? @"Microsoft.AspNetCore.Authentication.OpenIdConnect" : @"<PackageReference Include=""Microsoft.AspNetCore.Authentication.OpenIdConnect"" Version=""3.1.15"" />", csProjContent);
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

            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WebSocketClientProgram, @"\s", ""), Regex.Replace(clientProgramText, @"\s", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WebSocketServerProgram, @"\s", ""), Regex.Replace(serverProgramText, @"\s", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WebSocketServerStartup, @"\s", ""), Regex.Replace(serverStartupText, @"\s", ""));

            //Check that correct version is used
            Assert.True(clientProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
            Assert.True(serverProjContent.IndexOf(string.Concat(">", version, "<")) > 0);

            //Check that package has been added:
            StringAssert.Contains(@"Microsoft.AspNetCore.Diagnostics", serverProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Server.Kestrel", serverProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Hosting", serverProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", serverProjContent);

            Assert.True(serverProjContent.IndexOf(string.Concat(">", version, "<")) > 0);
        }

        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestOwinParadise(string version)
        {
            TestSolutionAnalysis results = AnalyzeSolution("OwinParadise.sln", tempDir, downloadLocation, version);

            var owinParadise = results.ProjectResults.Where(p => p.CsProjectPath.EndsWith("OwinParadise.csproj")).FirstOrDefault();
            FileAssert.Exists(owinParadise.CsProjectPath);

            var buildErrors = GetSolutionBuildErrors(results.SolutionRunResult.SolutionPath);
            Assert.AreEqual(0, buildErrors.Count);
        }

    }
}
