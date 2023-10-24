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
    public class WCFTests : AwsRulesBaseTest
    {
        public string ctaTestProjectsDir = "";
        public string downloadLocation;
        private Dictionary<string, TestSolutionAnalysis> _paCoreWCFSupportResultsDict;
        private Dictionary<string, TestSolutionAnalysis> _wcfTCPSelfHostResultsDict;

        private static IEnumerable<string> TestCases = SupportedFrameworks.GetSupportedFrameworksList();

        [OneTimeSetUp]
        public void Setup()
        {
            ctaTestProjectsDir = SetupTests.CtaTestProjectsDir;
            downloadLocation = SetupTests.DownloadLocation;

            var solutionName = "PACoreWCFSupport.sln";
            var net31Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Netcore31);
            var net50Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net5);
            var net60Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net6);
            var net70Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net7);
            var net80Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net8);
            _paCoreWCFSupportResultsDict = new Dictionary<string, TestSolutionAnalysis>
            {
                {SupportedFrameworks.Netcore31, net31Results},
                {SupportedFrameworks.Net5, net50Results},
                {SupportedFrameworks.Net6, net60Results},
                {SupportedFrameworks.Net7, net70Results},
                {SupportedFrameworks.Net8, net80Results}
            };
            
            solutionName = "WCFTCPSelfHost.sln";
            net31Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Netcore31);
            net50Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net5);
            net60Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net6);
            net70Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net7);
            net80Results = CopySolutionToUniqueTempDirAndAnalyze(solutionName, ctaTestProjectsDir, SupportedFrameworks.Net8);
            _wcfTCPSelfHostResultsDict = new Dictionary<string, TestSolutionAnalysis>
            {
                {SupportedFrameworks.Netcore31, net31Results},
                {SupportedFrameworks.Net5, net50Results},
                {SupportedFrameworks.Net6, net60Results},
                {SupportedFrameworks.Net7, net70Results},
                {SupportedFrameworks.Net8, net80Results}
            };
        }

        [Test, TestCaseSource("TestCases")]
        public void TestBasicHttpBindingAndTransportSecurity(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC2BasicHttpTransportSecurity";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartupWithBehavior, testCaseName, corewcfConfigPath), @"\r", "");
            
            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC2Program, @"\r", ""), 
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC2ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestBasicHttpTransportMessageCredUserName(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC3BasicHttpTransportMessageCredUserName";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartupWithBehavior, testCaseName, corewcfConfigPath), @"\r", "");

            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC3Program, @"\r", ""),
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC3ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestBasicHttpTransportMessageCredCertificate(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC4BasicHttpTransportMessageCredCertificate";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartupWithBehavior, testCaseName, corewcfConfigPath), @"\r", "");

            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC4Program, @"\r", ""),
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC4ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestWSHttpBindingWithWindowsAuth(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC5WSHttpBindingWithWindowsAuth";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartup, testCaseName, corewcfConfigPath), @"\r", "");

            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC5Program, @"\r", ""),
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC5ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestBasicHttpMessageSecurity(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith("TC6BasicHttpMessageSecurity.csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            StringAssert.DoesNotContain(@"CoreWCF.Primitives", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.Http", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.DoesNotContain(@"Microsoft.AspNetCore", csProjContent);

            FileAssert.DoesNotExist(Path.Combine(projectDir, "Startup.cs"));
            FileAssert.DoesNotExist(Path.Combine(projectDir, "Program.cs"));
        }

        [Test, TestCaseSource("TestCases")]
        public void TestNetTCPBindingDefaultSecurity(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC7NetTCPBindingDefaultSecurity";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartupWithBehavior, testCaseName, corewcfConfigPath), @"\r", "");

            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC7Program, @"\r", ""),
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC7ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestNetPipeBindingDefault(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith("TC8NetPipeBindingDefault.csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            StringAssert.DoesNotContain(@"CoreWCF.Primitives", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.Http", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.DoesNotContain(@"Microsoft.AspNetCore", csProjContent);

            FileAssert.DoesNotExist(Path.Combine(projectDir, "Startup.cs"));
            FileAssert.DoesNotExist(Path.Combine(projectDir, "Program.cs"));
        }

        [Test, TestCaseSource("TestCases")]
        public void TestBasicHttpAndNetTCPSupported(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC9BasicHttpAndNetTCPSupported";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartupWithBehavior, testCaseName, corewcfConfigPath), @"\r", "");

            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC9Program, @"\r", ""),
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC9ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestWsHttpAndNetPipe(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC10WsHttpAndNetPipe";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IService1.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");

            var corewcfConfigText = File.ReadAllText(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"CoreWCF.ConfigurationManager", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);

            string expectedStartup = Regex.Replace(string.Format(ExpectedOutputConstants.WCFConfigStartupWithBehavior, testCaseName, corewcfConfigPath), @"\r", "");

            StringAssert.AreEqualIgnoringCase(expectedStartup, Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC10Program, @"\r", ""),
                Regex.Replace(programText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(ExpectedOutputConstants.WCFTC10ConfigText, corewcfConfigText);
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestCodeBasedBasicHttpDefaultSecurity(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC1CodeBasicHttpDefaultSecurity";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IEchoService.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");
            FileAssert.DoesNotExist(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);

            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC1CodeStartupText, @"\r", ""), Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC1CodeBasedProgramText, @"\r", ""), Regex.Replace(programText, @"\r", ""));
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestCodeBasicHttpTransportSecurity(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC2CodeBasicHttpTransportSecurity";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IEchoService.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");
            FileAssert.DoesNotExist(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);

            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC2CodeBasedStartupText, @"\r", ""), Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC2CodeBasedProgramText, @"\r", ""), Regex.Replace(programText, @"\r", ""));
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestCodeBasicHttpTransportMessageCredUserName(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC3CodeBasicHttpTransportMessageCredUserName";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IEchoService.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");
            FileAssert.DoesNotExist(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);

            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC3CodeBasedStartupText, @"\r", ""), Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC3CodeBasedProgramText, @"\r", ""), Regex.Replace(programText, @"\r", ""));
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestCodeBasicHttpNetTCPSupported(string version)
        {
            var results = _paCoreWCFSupportResultsDict[version];

            var testCaseName = "TC9CodeBasicHttpNetTCPSupported";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupText = File.ReadAllText(Path.Combine(projectDir, "Startup.cs"));
            var programText = File.ReadAllText(Path.Combine(projectDir, "Program.cs"));

            var service = File.ReadAllText(Path.Combine(projectDir, "IEchoService.cs"));

            string corewcfConfigPath = Path.Combine(projectDir, "corewcf_ported.config");
            FileAssert.DoesNotExist(corewcfConfigPath);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);

            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC9CodeBasedStartupText, @"\r", ""), Regex.Replace(startupText, @"\r", ""));
            StringAssert.AreEqualIgnoringCase(Regex.Replace(ExpectedOutputConstants.WCFTC9CodeBasedProgramText, @"\r", ""), Regex.Replace(programText, @"\r", ""));
            StringAssert.Contains(@"using CoreWCF", service);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestWCFClient(string version)
        {
            var results = _wcfTCPSelfHostResultsDict[version];

            var testCaseName = "WcfTcpClient";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();

            var csProjContent = project.CsProjectContent;

            StringAssert.Contains(@"System.ServiceModel.Primitives", csProjContent);
            StringAssert.Contains(@"System.ServiceModel.Http", csProjContent);
            StringAssert.Contains(@"System.ServiceModel.NetTcp", csProjContent);
        }

        [Test, TestCaseSource("TestCases")]
        public void TestWCFServiceLibrary(string version)
        {
            var results = _wcfTCPSelfHostResultsDict[version];

            var testCaseName = "WcfServiceLibrary1";
            var project = results.ProjectResults.Where(prop => prop.CsProjectPath.EndsWith(testCaseName + ".csproj")).FirstOrDefault();
            var projectDir = Directory.GetParent(project.CsProjectPath).FullName;

            var csProjContent = project.CsProjectContent;

            var startupFile = Path.Combine(projectDir, "Startup.cs");
            var programFile = Path.Combine(projectDir, "Program.cs");

            FileAssert.DoesNotExist(startupFile);
            FileAssert.DoesNotExist(programFile);

            StringAssert.Contains(@"CoreWCF.Primitives", csProjContent);
            StringAssert.Contains(@"CoreWCF.Http", csProjContent);
            StringAssert.Contains(@"CoreWCF.NetTcp", csProjContent);
            StringAssert.Contains(@"Microsoft.AspNetCore", csProjContent);
            StringAssert.DoesNotContain(@"CoreWCF.ConfigurationManager", csProjContent);
        }
    }
}
