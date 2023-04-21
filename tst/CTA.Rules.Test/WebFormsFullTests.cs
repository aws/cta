using System.Collections.Generic;
using CTA.Rules.Test.Models;
using NUnit.Framework;
using System.IO;
using System.Linq;
using CTA.Rules.Models;

namespace CTA.Rules.Test
{
    /// <summary>
    /// This class is exclusively for testing WebFormsFull.sln,
    /// a solution with a WebForms project containing all supported
    /// WebForms porting rules.
    /// </summary>
    [TestFixture]
    class WebFormsFullTests : AwsRulesBaseTest
    {
        public string ctaTestProjectsDir = "";
        public string downloadLocation;
        private Dictionary<string, TestSolutionAnalysis> _resultsDict;

        private static IEnumerable<string> TestCases = SupportedFrameworks.GetSupportedFrameworksList();

        [OneTimeSetUp]
        public void Setup()
        {
            ctaTestProjectsDir = SetupTests.CtaTestProjectsDir;
            downloadLocation = SetupTests.DownloadLocation;

            var solutionName = "WebFormsFull.sln";
            var net31Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Netcore31);
            var net50Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net5);
            var net60Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net6);
            var net70Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net7);

            _resultsDict = new Dictionary<string, TestSolutionAnalysis>
            {
                {SupportedFrameworks.Netcore31, net31Results},
                {SupportedFrameworks.Net5, net50Results},
                {SupportedFrameworks.Net6, net60Results},
                {SupportedFrameworks.Net7, net70Results}
            };
        }

        [TestCase(SupportedFrameworks.Netcore31)]
        [TestCase(SupportedFrameworks.Net5)]
        [TestCase(SupportedFrameworks.Net6)]
        public void TestProjectFilePortingResults(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));

            // Verify expected packages are in .csproj
            StringAssert.Contains($"<TargetFramework>{version}</TargetFramework>", webFormsFullResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.12"" />", webFormsFullResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Newtonsoft.Json"" Version=""*"" />", webFormsFullResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.Data.SqlClient"" Version=""5.0.1"" />", webFormsFullResult.CsProjectContent);
        }

        [TestCase(SupportedFrameworks.Net7)]
        public void TestProjectFilePortingResults_Dotnet7AndAbove(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));

            // Verify expected packages are in .csproj
            StringAssert.Contains($"<TargetFramework>{version}</TargetFramework>", webFormsFullResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""*"" />", webFormsFullResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Newtonsoft.Json"" Version=""*"" />", webFormsFullResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.Data.SqlClient"" Version=""5.0.1"" />", webFormsFullResult.CsProjectContent);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestUserControlsPortingResults(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Component file exists
            FileAssert.Exists(Path.Combine(projectDir, "Components", "ViewSwitcher.razor"));

            // Component code-behind contents
            var viewSwitcherRazorCs = File.ReadAllText(Path.Combine(projectDir, "Components", "ViewSwitcher.razor.cs"));
            StringAssert.Contains("using Microsoft.AspNetCore.Components;", viewSwitcherRazorCs);
            StringAssert.Contains("public partial class ViewSwitcher : ComponentBase", viewSwitcherRazorCs);
            StringAssert.DoesNotContain("public partial class ViewSwitcher : System.Web.UI.UserControl", viewSwitcherRazorCs);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestStaticFilesMovedToWwwroot(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);
            var wwwrootDir = Path.Combine(projectDir, "wwwroot");
            var contentDir = Path.Combine(wwwrootDir, "Content");
            var fontsDir = Path.Combine(wwwrootDir, "fonts");
            var imagesDir = Path.Combine(wwwrootDir, "images");
            var picsDir = Path.Combine(wwwrootDir, "Pics");
            var scriptsDir = Path.Combine(wwwrootDir, "Scripts");

            // Verify static files are migrated to wwwroot dir
            Assert.AreEqual(1, Directory.EnumerateFiles(wwwrootDir).Count());
            Assert.AreEqual(15, Directory.EnumerateFiles(contentDir).Count());
            Assert.AreEqual(10, Directory.EnumerateFiles(fontsDir).Count());
            Assert.AreEqual(4, Directory.EnumerateFiles(imagesDir).Count());
            Assert.AreEqual(13, Directory.EnumerateFiles(picsDir).Count());
            Assert.AreEqual(18, Directory.EnumerateFiles(scriptsDir, "*", SearchOption.AllDirectories).Count());
        }

        [Test, TestCaseSource("TestCases")]
        public void TestProperHttpModulePortsCorrectly(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);
            
            // HttpModule contents
            var content = File.ReadAllText(Path.Combine(projectDir, "Middleware", "HttpModules", "TestProperHttpModule.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.TestProperHttpModuleFile, content);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestProperHttpModuleAlternatePortsCorrectly(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Application_ResolveRequestCache generates its own module
            var content = File.ReadAllText(Path.Combine(projectDir, "Middleware", "HttpModules", "TestProperHttpModuleAlternateResolveRequestCache.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.TestProperHttpModuleAlternateResolveRequestCacheFile, content);

            //  Application_PostResolveRequestCache generates its own module
            var spinoffContent = File.ReadAllText(Path.Combine(projectDir, "Middleware", "HttpModules", "TestProperHttpModuleAlternatePostResolveRequestCache.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.TestProperHttpModuleAlternatePostResolveRequestCacheFile, spinoffContent);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestImproperHttpModulePortsCorrectly(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // HttpModule contents
            var content = File.ReadAllText(Path.Combine(projectDir, "Middleware", "HttpModules", "TestImproperHttpModule.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.TestImproperHttpModuleFile, content);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestHttpHandlersMovedToMiddleware(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);
            
            // HttpModule contents
            var content = File.ReadAllText(Path.Combine(projectDir, "Middleware", "HttpHandlers", "TestHttpHandler.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.TestHttpHandlerFile, content);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestDefaultAspxMigrations(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);
            
            var defaultRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Default.razor"));
            StringAssert.DoesNotContain("asp:Content", defaultRazor);
            StringAssert.Contains(@"@page ""/""", defaultRazor);
            StringAssert.Contains("@inherits WebFormsFull._Default", defaultRazor);
            StringAssert.DoesNotContain("asp:Button", defaultRazor);
            StringAssert.Contains("Button", defaultRazor);
            StringAssert.DoesNotContain("asp:Label", defaultRazor);
            StringAssert.Contains("label", defaultRazor);
            StringAssert.DoesNotContain("asp:Hyperlink", defaultRazor);
            StringAssert.Contains("<a", defaultRazor);
            StringAssert.DoesNotContain("asp:GridView", defaultRazor);
            StringAssert.Contains("GridView", defaultRazor);
            StringAssert.DoesNotContain("asp:BoundField", defaultRazor);
            StringAssert.Contains("BoundField", defaultRazor);
            StringAssert.DoesNotContain("asp:ListView", defaultRazor);
            StringAssert.Contains("ListView", defaultRazor);
            StringAssert.DoesNotContain("asp:PlaceHolder", defaultRazor);
            StringAssert.Contains("@itemPlaceHolder", defaultRazor);
            StringAssert.Contains("EmptyDataTemplate", defaultRazor);
            StringAssert.Contains("LayoutTemplate", defaultRazor);
            StringAssert.Contains("DataSource=\"@(PeopleGrid_DataSource)\"", defaultRazor);
            StringAssert.Contains("DataSource=\"@(productList_DataSource)\"", defaultRazor);
            StringAssert.DoesNotContain("<%#", defaultRazor);
            StringAssert.DoesNotContain("%>", defaultRazor);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestOtherPageAspxMigration(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            var otherPageRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "OtherPage.razor"));
            StringAssert.Contains(@"@page ""/OtherPage""", otherPageRazor);
            StringAssert.Contains(@"@layout Site", otherPageRazor);
            StringAssert.Contains(@"@inherits WebFormsFull.OtherPage", otherPageRazor);
            StringAssert.DoesNotContain("<%: Title %>", otherPageRazor);
            StringAssert.Contains(@"@(Title)", otherPageRazor);
            StringAssert.DoesNotContain("<br />", otherPageRazor);
            StringAssert.Contains(@"<br>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:TextBox ID=""SLTTB"" Text=""Single Line Textbox"" TextMode=""SingleLine""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:TextBox ID=""PWTB"" TextMode=""Password""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:TextBox ID=""MLTB"" Text=""Multi Line TextBox"" TextMode=""MultiLine""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:RadioButton ID=""RadioTest11"" GroupName=""TestGroup1"" Text=""Radio Test 1-1""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:RadioButton ID=""RadioTest12"" GroupName=""TestGroup1"" Text=""Radio Test 1-2""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:RadioButton ID=""RadioTest13"" GroupName=""TestGroup1"" Text=""Radio Test 1-3""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:RadioButton ID=""RadioTest21"" GroupName=""TestGroup2"" Text=""Radio Test 2-1""/>", otherPageRazor);
            StringAssert.DoesNotContain(@"<asp:RadioButton ID=""RadioTest22"" GroupName=""TestGroup2"" Text=""Radio Test 2-2""/>", otherPageRazor);
            StringAssert.Contains(@"<input id=""SLTTB"" value=""Single Line Textbox"" type=""text"">", otherPageRazor);
            StringAssert.Contains(@"<input id=""PWTB"" type=""password"">", otherPageRazor);
            StringAssert.Contains(@"<textarea id=""MLTB"">", otherPageRazor);
            StringAssert.Contains(@"<input id=""RadioTest11"" type=""radio"" name=""TestGroup1"">", otherPageRazor);
            StringAssert.Contains(@"<label for=""RadioTest11"">", otherPageRazor);
            StringAssert.Contains(@"<input id=""RadioTest12"" type=""radio"" name=""TestGroup1"">", otherPageRazor);
            StringAssert.Contains(@"<label for=""RadioTest12"">", otherPageRazor);
            StringAssert.Contains(@"<input id=""RadioTest13"" type=""radio"" name=""TestGroup1"">", otherPageRazor);
            StringAssert.Contains(@"<label for=""RadioTest13"">", otherPageRazor);
            StringAssert.Contains(@"<input id=""RadioTest21"" type=""radio"" name=""TestGroup2"">", otherPageRazor);
            StringAssert.Contains(@"<label for=""RadioTest21"">", otherPageRazor);
            StringAssert.Contains(@"<input id=""RadioTest22"" type=""radio"" name=""TestGroup2"">", otherPageRazor);
            StringAssert.Contains(@"<label for=""RadioTest22"">", otherPageRazor);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestDefaultAspxCsMigrations(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);
            
            // Verify code-behind files
            var defaultRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Default.razor.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.DefaultRazorCsFile, defaultRazorCs);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestOtherPageAspxCsMigrations(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);
            
            // Verify code-behind files
            var otherPageRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "OtherPage.razor.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.OtherPageRazorCsFile, otherPageRazorCs);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestHostFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var hostFile = File.ReadAllText(Path.Combine(projectDir, "Pages", "_Host.cshtml"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.HostFile, hostFile);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestImportsFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var importsFile = File.ReadAllText(Path.Combine(projectDir, "_Imports.razor"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.ImportsFile, importsFile);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestActivityIdHelperFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var activityIdHelperFile = File.ReadAllText(Path.Combine(projectDir, "ActivityIdHelper.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.ActivityIdHelperFile, activityIdHelperFile);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestAppFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var appFile = File.ReadAllText(Path.Combine(projectDir, "App.razor"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.AppFile, appFile);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestAppSettingsFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var appSettingsFile = File.ReadAllText(Path.Combine(projectDir, "appsettings.json"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.AppSettings, appSettingsFile);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestProgramFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var programFile = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.ProgramFile, programFile);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestStartupFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var startupFile = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.StartupFile.Replace("\r\n", "\n"), startupFile.Replace("\r\n", "\n"));
        }

        [Test, TestCaseSource("TestCases")]
        public void TestWebRequestInfoFileCreation(string version)
        {
            var results = _resultsDict[version];
            var webFormsFullResult =
                results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("WebFormsFull.csproj"));
            var projectDir = Path.GetDirectoryName(webFormsFullResult.CsProjectPath);

            // Blazor files and contents
            var webRequestInfoFile = File.ReadAllText(Path.Combine(projectDir, "WebRequestInfo.cs"));
            StringAssert.AreEqualIgnoringCase(WebFormsFullExpectedOutputConstants.WebRequestInfoFile, webRequestInfoFile);
        }
    }
}
