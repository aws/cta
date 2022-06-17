using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Update;
using NUnit.Framework;

namespace CTA.Rules.Test
{
    internal class VisualBasicTests : AwsRulesBaseTest
    {
        public string downloadLocation;
        public List<string> ctaFiles;
        public string version = "net5.0"; //We don't care about version for CTA-only rules:

        [SetUp]
        public void Setup()
        {
            downloadLocation = SetupTests.DownloadLocation;
            ctaFiles = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CTAFiles")), "*.json")
               .Select(s => Path.GetFileNameWithoutExtension(s))
               .ToList();
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
                RulesDir = Constants.RulesDefaultPath,
                AdditionalReferences = ctaFiles
            };
            
            List<ProjectConfiguration> solutionConfiguration = new List<ProjectConfiguration>
            {
                projectConfiguration
            };
            CopyTestRules();

            SolutionRewriter solutionRewriter = new SolutionRewriter(solutionPath, solutionConfiguration, language: LanguageOptions.Vb);
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
        public void TestOwinParadiseVb()
        {
            var slnResults = runCTAFile("OwinParadiseVb.sln");
            var projresults = slnResults.ProjectResults.FirstOrDefault();
            Assert.IsTrue(projresults != null);

            StringAssert.Contains("Microsoft.AspNetCore.Hosting", projresults.ProjectAnalysisResult);

            var signalR = File.ReadAllText(Path.Combine(projresults.ProjectDirectory, "SignalR.vb"));
            var startUp = File.ReadAllText(Path.Combine(projresults.ProjectDirectory, "Startup.vb"));

            //Check that namespace has been added
            StringAssert.Contains(@"Microsoft.AspNetCore.Owin", startUp);
            StringAssert.DoesNotContain("Imports Owin", startUp);
            StringAssert.Contains("Imports Microsoft.AspNetCore.Hosting", signalR);
            StringAssert.DoesNotContain("Imports Microsoft.Owin.Hosting", signalR);

            //Check identifier actions
            StringAssert.Contains("IApplicationBuilder", startUp);
            StringAssert.DoesNotContain("IAppBuilder", startUp);
            StringAssert.Contains("IApplicationBuilder", signalR);
            StringAssert.DoesNotContain("IAppBuilder", signalR);

            //Check method actions
            StringAssert.Contains("UseEndpoints", signalR);
        }

        public void TestVbNetMvc()
        {
            var results = runCTAFile("VBNetMvc.sln").ProjectResults.FirstOrDefault();
            // Check that nothing is ported.
        }
    }
}
