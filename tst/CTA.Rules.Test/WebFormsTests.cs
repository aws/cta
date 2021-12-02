using CTA.Rules.Test.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
    }
}
