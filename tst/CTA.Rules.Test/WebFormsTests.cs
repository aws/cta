using CTA.Rules.Test.Models;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using Microsoft.Extensions.Logging;

namespace CTA.Rules.Test
{
    class WebFormsTests : AwsRulesBaseTest
    {
        public string tempDir = "";
        public string downloadLocation;

        [SetUp]
        public void Setup()
        {
            tempDir = SetupTests.TempDir;
            downloadLocation = SetupTests.DownloadLocation;
        }

        [TestCase(TargetFramework.Dotnet6)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.DotnetCoreApp31)]
        public void TestAspNetWebFormsProject(string version)
        {
            var solutionPath = CopySolutionFolderToTemp("ASP.NET-WebForms.sln", tempDir);
            TestSolutionAnalysis results = AnalyzeSolution(solutionPath, version);

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

        [TestCase(TargetFramework.DotnetCoreApp31)]
        [TestCase(TargetFramework.Dotnet5)]
        [TestCase(TargetFramework.Dotnet6)]
        public async Task TestSolutionWithMvcAndDualWebForms(string version)
        {
            var solutionName = "MvcAndDualWebForms.sln";
            var solutionPath = CopySolutionFolderToTemp(solutionName, tempDir);
            var results = AnalyzeSolution(solutionPath, version);

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
        }

        public void VerifyWebForms(string targetFramework, ProjectResult webFormsProjectResult)
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
    }
}
