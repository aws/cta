using CTA.Rules.Models;
using CTA.Rules.Test.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CTA.Rules.Test
{
    [TestFixture]
    class WebFormsTests : AwsRulesBaseTest
    {
        public string ctaTestProjectsDir = "";
        public string downloadLocation;
        private Dictionary<string, TestSolutionAnalysis> _aspnetWebFormsSolution;
        private Dictionary<string, TestSolutionAnalysis> _mvcAndDualWebFormsSolution;

        private static IEnumerable<string> TestCases = SupportedFrameworks.GetSupportedFrameworksList();

        [OneTimeSetUp]
        public void Setup()
        {
            ctaTestProjectsDir = SetupTests.CtaTestProjectsDir;
            downloadLocation = SetupTests.DownloadLocation;

            var aspnetWebFormsSolutionName = "ASP.NET-WebForms.sln";
            var net31Results = CopySolutionToUniqueTempDirAndAnalyze(aspnetWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Netcore31);
            var net50Results = CopySolutionToUniqueTempDirAndAnalyze(aspnetWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net5);
            var net60Results = CopySolutionToUniqueTempDirAndAnalyze(aspnetWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net6);
            var net70Results = CopySolutionToUniqueTempDirAndAnalyze(aspnetWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net7);
            var net80Results = CopySolutionToUniqueTempDirAndAnalyze(aspnetWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net8);

            _aspnetWebFormsSolution = new Dictionary<string, TestSolutionAnalysis>
            {
                {SupportedFrameworks.Netcore31, net31Results},
                {SupportedFrameworks.Net5, net50Results},
                {SupportedFrameworks.Net6, net60Results},
                {SupportedFrameworks.Net7, net70Results},
                {SupportedFrameworks.Net8, net80Results}
            };

            var mvcDualWebFormsSolutionName = "MvcAndDualWebForms.sln";
            net31Results = CopySolutionToUniqueTempDirAndAnalyze(mvcDualWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Netcore31);
            net50Results = CopySolutionToUniqueTempDirAndAnalyze(mvcDualWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net5);
            net60Results = CopySolutionToUniqueTempDirAndAnalyze(mvcDualWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net6);
            net70Results = CopySolutionToUniqueTempDirAndAnalyze(mvcDualWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net7);
            net80Results = CopySolutionToUniqueTempDirAndAnalyze(mvcDualWebFormsSolutionName, ctaTestProjectsDir, SupportedFrameworks.Net8);
            
            _mvcAndDualWebFormsSolution = new Dictionary<string, TestSolutionAnalysis>
            {
                {SupportedFrameworks.Netcore31, net31Results},
                {SupportedFrameworks.Net5, net50Results},
                {SupportedFrameworks.Net6, net60Results},
                {SupportedFrameworks.Net7, net70Results},
                {SupportedFrameworks.Net8, net80Results}
            };
        }

        [Test, TestCaseSource("TestCases")]
        public void TestAspNetWebFormsProject(string version)
        {
            var results = _aspnetWebFormsSolution[version];

            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith("ASP.NET-WebForms" + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WebFormsStartupText, @"\r|\n|\t", ""),
                Regex.Replace(startupText, @"\r|\n|\t", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WebFormsProgramText, @"\r|\n|\t", ""),
                Regex.Replace(programText, @"\r|\n|\t", ""));
        }

        [TestCase(SupportedFrameworks.Netcore31)]
        [TestCase(SupportedFrameworks.Net5)]
        [TestCase(SupportedFrameworks.Net6)]
        [TestCase(SupportedFrameworks.Net7)]
        public void TestSolutionWithMvcAndDualWebForms(string version)
        {
            var results = _mvcAndDualWebFormsSolution[version];

            var someMvcResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("SomeMvc.csproj"));
            var someWebFormsResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("SomeWebForms.csproj"));
            var otherWebFormsResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("OtherWebForms.csproj"));

            // Verify MVC porting is unaffected
            StringAssert.Contains($"<TargetFramework>{version}</TargetFramework>", someMvcResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.12"" />", someMvcResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Newtonsoft.Json"" Version=""*"" />", someMvcResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Antlr3.Runtime"" Version=""*"" />", someMvcResult.CsProjectContent);

            // Verify both WebForms projects ported as expected
            VerifyWebForms(version, someWebFormsResult);
            VerifyWebForms(version, otherWebFormsResult);

            // Verify that MVC did not have WebForms porting applied to it
            VerifyMvc(someMvcResult);
        }

        [TestCase(SupportedFrameworks.Net8)]
        public void TestSolutionWithMvcAndDualWebForms_Dotnet8AndAbove(string version)
        {
            var results = _mvcAndDualWebFormsSolution[version];

            var someMvcResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("SomeMvc.csproj"));
            var someWebFormsResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("SomeWebForms.csproj"));
            var otherWebFormsResult = results.ProjectResults.First(proj => proj.CsProjectPath.EndsWith("OtherWebForms.csproj"));

            // Verify MVC porting is unaffected
            StringAssert.Contains($"<TargetFramework>{version}</TargetFramework>", someMvcResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""*"" />", someMvcResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Newtonsoft.Json"" Version=""*"" />", someMvcResult.CsProjectContent);
            StringAssert.Contains(@"<PackageReference Include=""Antlr3.Runtime"" Version=""*"" />", someMvcResult.CsProjectContent);

            // Verify both WebForms projects ported as expected
            VerifyWebForms_Dotnet7AndAbove(version, someWebFormsResult);
            VerifyWebForms_Dotnet7AndAbove(version, otherWebFormsResult);

            // Verify that MVC did not have WebForms porting applied to it
            VerifyMvc(someMvcResult);
        }

        private void VerifyWebForms(string targetFramework, ProjectResult webFormsProjectResult)
        {
            var csProjContent = webFormsProjectResult.CsProjectContent;
            var projectDir = Path.GetDirectoryName(webFormsProjectResult.CsProjectPath);

            // Validate csproj file contents
            StringAssert.Contains($"<TargetFramework>{targetFramework}</TargetFramework>", csProjContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.12"" />", csProjContent);
            StringAssert.Contains(@"<PackageReference Include=""Newtonsoft.Json"" Version=""*"" />", csProjContent);

            // Validate code changes
            // Verify host file
            var hostFile = File.ReadAllText(Path.Combine(projectDir, "Pages", "_Host.cshtml"));
            StringAssert.Contains(@"<app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>", hostFile);

            // Verify view files
            var aboutRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "About.razor"));
            var contactRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Contact.razor"));
            var defaultRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Default.razor"));
            var detailsRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Catalog", "Details.razor"));
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
            var aboutRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "About.razor.cs"));
            var contactRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Contact.razor.cs"));
            var defaultRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Default.razor.cs"));
            var detailsRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Catalog", "Details.razor.cs"));
            StringAssert.Contains("protected override void OnInitialized()", aboutRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", contactRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", defaultRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", detailsRazorCs);

            // Verify application files
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var webRequestInfoText = File.ReadAllText(Path.Combine(projectDir, "WebRequestInfo.cs"));
            StringAssert.DoesNotContain("public class WebRequestInfo", startupText);
            StringAssert.Contains(ExpectedOutputConstants.BlazorConfigureFunctionText, startupText);
            StringAssert.Contains(ExpectedOutputConstants.BlazorProgramText, programText);
            StringAssert.Contains(ExpectedOutputConstants.BlazorWebRequestInfoText, webRequestInfoText);
        }

        private void VerifyWebForms_Dotnet7AndAbove(string targetFramework, ProjectResult webFormsProjectResult)
        {
            var csProjContent = webFormsProjectResult.CsProjectContent;
            var projectDir = Path.GetDirectoryName(webFormsProjectResult.CsProjectPath);

            // Validate csproj file contents
            StringAssert.Contains($"<TargetFramework>{targetFramework}</TargetFramework>", csProjContent);
            StringAssert.Contains(@"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""*"" />", csProjContent);
            StringAssert.Contains(@"<PackageReference Include=""Newtonsoft.Json"" Version=""*"" />", csProjContent);

            // Validate code changes
            // Verify host file
            var hostFile = File.ReadAllText(Path.Combine(projectDir, "Pages", "_Host.cshtml"));
            StringAssert.Contains(@"<app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>", hostFile);

            // Verify view files
            var aboutRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "About.razor"));
            var contactRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Contact.razor"));
            var defaultRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Default.razor"));
            var detailsRazor = File.ReadAllText(Path.Combine(projectDir, "Pages", "Catalog", "Details.razor"));
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
            var aboutRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "About.razor.cs"));
            var contactRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Contact.razor.cs"));
            var defaultRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Default.razor.cs"));
            var detailsRazorCs = File.ReadAllText(Path.Combine(projectDir, "Pages", "Catalog", "Details.razor.cs"));
            StringAssert.Contains("protected override void OnInitialized()", aboutRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", contactRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", defaultRazorCs);
            StringAssert.Contains("protected override void OnInitialized()", detailsRazorCs);

            // Verify application files
            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));
            var webRequestInfoText = File.ReadAllText(Path.Combine(projectDir, "WebRequestInfo.cs"));
            StringAssert.DoesNotContain("public class WebRequestInfo", startupText);
            StringAssert.Contains(ExpectedOutputConstants.BlazorConfigureFunctionText, startupText);
            StringAssert.Contains(ExpectedOutputConstants.BlazorProgramText, programText);
            StringAssert.Contains(ExpectedOutputConstants.BlazorWebRequestInfoText, webRequestInfoText);
        }

        public void VerifyMvc(ProjectResult mvcProjectResult)
        {
            var projectDir = Path.GetDirectoryName(mvcProjectResult.CsProjectPath);

            // These files will only exist if WebForms porting was run on the project
            var appRazorLocation = Path.Combine(projectDir, "App.razor");
            var importsRazorLocation = Path.Combine(projectDir, "_Imports.razor");
            var hostCshtmlLocation = Path.Combine(projectDir, "Pages", "_Host.cshtml");

            Assert.False(File.Exists(appRazorLocation));
            Assert.False(File.Exists(importsRazorLocation));
            Assert.False(File.Exists(hostCshtmlLocation));
        }
    }
}
