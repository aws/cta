using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CTA.Rules.Test
{
    internal class VisualBasicTests : AwsRulesBaseTest
    {
        private string _tempDir;
        private string _downloadLocation;
        private List<string> _ctaFiles;
        private readonly string _version = "net5.0"; 
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
            StringAssert.Contains("net5.0", projectFile);
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
            
            // uncomment once template in datastore is merged.
            // StringAssert.Contains(
            //     "<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>",
            //     results.CsProjectContent);
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
            Assert.IsTrue(projresults.All(content => content.Contains("net5.0")));
            Assert.IsTrue(slnResults.ProjectResults
                .Find(p => p.CsProjectPath.EndsWith(".vbproj"))
                .CsProjectContent.Contains("BouncyCastle.NetCore"));
        }
    }
}
