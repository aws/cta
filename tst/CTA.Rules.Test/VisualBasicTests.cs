using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Analyzer;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CTA.Rules.Test
{
    internal class VisualBasicTests : AwsRulesBaseTest
    {
        private string _tempDir;
        private string _downloadLocation;
        private List<string> _ctaFiles;
        private readonly string _version = "net6.0";
        //We don't care about version for CTA-only rules:

        [SetUp]
        public void Setup()
        {
            _tempDir = SetupTests.TempDir;
            _downloadLocation = SetupTests.DownloadLocation;
            _ctaFiles = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CTAFiles")), "*.json")
               .Select(s => Path.GetFileNameWithoutExtension(s))
               .ToList();
        }

        [Test]
        public void TestOwinParadiseVb()
        {
            var slnResults = AnalyzeSolution("OwinParadiseVb.sln", _tempDir, _downloadLocation, _version);
            var projresults = slnResults.ProjectResults.FirstOrDefault();
            Assert.IsTrue(projresults != null);

            StringAssert.Contains("Microsoft.AspNetCore.Hosting", projresults.ProjectAnalysisResult);

            var signalR = File.ReadAllText(Path.Combine(projresults.ProjectDirectory, "SignalR.vb"));
            var startUp = File.ReadAllText(Path.Combine(projresults.ProjectDirectory, "Startup.vb"));
            var projectFile = File.ReadAllText(projresults.CsProjectPath);

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

            //Check project porting
            StringAssert.Contains("net6.0", projectFile);
            StringAssert.Contains("Microsoft.AspNetCore.Diagnostics", projectFile);
        }

        [Test]
        public void TestVbNetMvc()
        {
            var results = AnalyzeSolution("VBNetMvc.sln",
                    _tempDir,
                    _downloadLocation,
                    _version)
                .ProjectResults.FirstOrDefault();
            // Check that nothing is ported.
            StringAssert.Contains(
                "<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>",
                results.CsProjectContent);
        }


        [Test]
        public void TestMixedClassLibrary()
        {
            var slnResults = AnalyzeSolution("MixedClassLibrary.sln",
                _tempDir,
                _downloadLocation,
                _version);
            var projresults = slnResults.ProjectResults.Select(p => p.CsProjectContent).ToList();
            Assert.IsTrue(projresults != null);
            Assert.IsTrue(projresults.Count() == 2);
            //check both projects ported
            Assert.IsTrue(projresults.All(content => content.Contains("net6.0")));
            Assert.IsTrue(slnResults.ProjectResults
                .Find(p => p.CsProjectPath.EndsWith(".vbproj"))
                .CsProjectContent.Contains("BouncyCastle.NetCore"));
        }

        [Test]
        public async Task RunMixedUsingGenerator()
        {
            var solutionPath = CopySolutionFolderToTemp("MixedClassLibrary.sln", _tempDir);

            AnalyzerConfiguration configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings =
                {
                    GenerateJsonOutput = false,
                    OutputPath = @"/tmp/UnitTests"
                },

                MetaDataSettings =
                {
                    LiteralExpressions = true,
                    MethodInvocations = true,
                    Annotations = true,
                    DeclarationNodes = true,
                    LocationData = false,
                    ReferenceData = true,
                    LoadBuildData = true,
                    ElementAccess = true,
                    MemberAccess = true
                }
            };

            //CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(configuration, NullLogger.Instance);
            CodeAnalyzerByLanguage analyzer = new CodeAnalyzerByLanguage(configuration, NullLogger.Instance);

            SolutionPort solutionPort = new SolutionPort(solutionPath);

            var resultEnumerator = analyzer.AnalyzeSolutionGeneratorAsync(solutionPath).GetAsyncEnumerator();

            while (await resultEnumerator.MoveNextAsync())
            {
                using var result = resultEnumerator.Current;
                PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
                {
                    SolutionPath = solutionPath,
                    ProjectPath = result.ProjectResult.ProjectFilePath,
                    IsMockRun = false,
                    UseDefaultRules = true,
                    PortCode = true,
                    PortProject = true,
                    TargetVersions = new List<string> { _version }
                };

                solutionPort.RunProject(result, projectConfiguration);
            }
            var portSolutionResult = solutionPort.GenerateResults();
            var testSolutionResult = GenerateSolutionResult(solutionPath, solutionPort.GetAnalysisResult(), portSolutionResult);
            var projResults = testSolutionResult.ProjectResults.Select(p => p.CsProjectContent).ToList();
            Assert.IsTrue(projResults != null);
            Assert.IsTrue(projResults.Count() == 2);
            //check both projects ported
            Assert.IsTrue(projResults.All(content => content.Contains("net6.0")));
            Assert.IsTrue(testSolutionResult.ProjectResults
                .Find(p => p.CsProjectPath.EndsWith(".vbproj"))
                .CsProjectContent.Contains("BouncyCastle.NetCore"));
        }
    }
}
