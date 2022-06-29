using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Tests.Utils;
using CTA.Rules.Config;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CTA.FeatureDetection.Tests
{
    [SetUpFixture]
    public class TestProjectsSetupFixture
    {
        private string _tempDirName;
        private string _tempBaseDir;
        private static string _targetDir;
        private static readonly string _coreMvcDir = "CoreMVC";
        private static readonly string _coreWebApiDir = "CoreWebApi";
        private static readonly string _efDir = "Ef";
        private static readonly string _mvcDir = "Mvc";
        private static readonly string _webApiDir = "WebApi";
        private static readonly string _webFormsDir = "WebForms";
        private static readonly string _webClassLibraryDir = "WebClassLibrary";
        private static readonly string _windowsAuthenticationDir = "WindowsAuthentication";
        private static readonly string _iisConfigDir = "IIS";
        private static readonly string _formsAuthenticationDir = "FormsAuthentication";
        private static readonly string _federatedAuthenticationDir = "FederatedAuthentication";
        private static readonly string _wcfDir = "WCF";
        private static readonly string _wcfConfigBasedProjectDir = "WCFConfigBasedProject";
        private static readonly string _wcfCodeBasedProjectDir = "WCFCodeBasedProject";
        private static readonly string _vbClassLibraryDir = "VBClassLibrary";
        private static readonly string _vbWebApiDir = "VBWebApi";
        private static readonly string _vbNetMvcDir = "VBNetMvc";
        private static readonly string _vbWebFormsDir = "VBWebForms";
        private static readonly string TestProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));
        public static string ConfigFile => Path.Combine(TestProjectDirectory, "Examples", "Templates", "feature_config.json");

        public static string CoreMvcSolutionName => "CoreMVC.sln";
        public static string CoreWebApiSolutionName => "CoreWebApi.sln";
        public static string VBClassLibrarySolutionName => "VBClassLibrary.sln";
        public static string VBWebApiSolutionName => "VBWebApi.sln";
        public static string VBWebFormsSolutionName => "VBWebForms.sln";
        public static string VBNetMvcSolutionName => "VBNetMvc.sln";
        public static string EfSolutionName => "EF6_Test.sln";
        public static string MvcSolutionName => "ASP.NET-MVC-Framework.sln";
        public static string WebApiSolutionName => "WebApi-Framework.sln";
        public static string WebFormsSolutionName => "ASP.NET-WebForms.sln";
        public static string WebClassLibrarySolutionName => "WebClassLibrary.sln";
        public static string WindowsAuthenticationSolutionName => "WindowsAuthentication.sln";
        public static string IISConfigSolutionName => "MVCAppWithIISConfig.sln";
        public static string FormsAuthenticationSolutionName => "FormsAuthentication.sln";
        public static string FederatedAuthenticationSolutionName => "FederatedAuthentication.sln";
        public static string CoreWCFServiceSolutionName => "WCFServer.sln";
        public static string WCFClientSolutionName => "WCFClient.sln";
        public static string CoreMvcSolutionPath => Path.Combine(_targetDir, _coreMvcDir, CoreMvcSolutionName);
        public static string CoreWebApiSolutionPath => Path.Combine(_targetDir, _coreWebApiDir, CoreWebApiSolutionName);
        public static string VBClassLibrarySolutionPath => Path.Combine(_targetDir, _vbClassLibraryDir, VBClassLibrarySolutionName);
        public static string VBWebApiSolutionPath => Path.Combine(_targetDir, _vbWebApiDir, VBWebApiSolutionName);
        public static string VBWebFormsSolutionPath => Path.Combine(_targetDir, _vbWebFormsDir, VBWebFormsSolutionName);
        public static string VBNetMvcSolutionPath => Path.Combine(_targetDir, _vbNetMvcDir, VBNetMvcSolutionName);
        public static string EfSolutionPath => Path.Combine(_targetDir, _efDir, EfSolutionName);
        public static string MvcSolutionPath => Path.Combine(_targetDir, _mvcDir, MvcSolutionName);
        public static string WebFormsSolutionPath => Path.Combine(_targetDir, _webFormsDir, WebFormsSolutionName);
        public static string WebApiSolutionPath => Path.Combine(_targetDir, _webApiDir, WebApiSolutionName);
        public static string WebClassLibrarySolutionPath => Path.Combine(_targetDir, _webClassLibraryDir, WebClassLibrarySolutionName);
        public static string WindowsAuthenticationSolutionPath => Path.Combine(_targetDir, _windowsAuthenticationDir, WindowsAuthenticationSolutionName);
        public static string FormsAuthenticationSolutionPath => Path.Combine(_targetDir, _formsAuthenticationDir, FormsAuthenticationSolutionName);
        public static string IISConfigSolutionPath => Path.Combine(_targetDir, _iisConfigDir, IISConfigSolutionName);
        public static string FederatedAuthenticationSolutionPath => Path.Combine(_targetDir, _federatedAuthenticationDir, FederatedAuthenticationSolutionName);
        public static string CoreWCFServiceSolutionPath => Path.Combine(_targetDir, _wcfDir, CoreWCFServiceSolutionName);
        public static string WCFClientSolutionPath => Path.Combine(_targetDir, _wcfDir, WCFClientSolutionName);
        public static string CoreMvcProjectPath => Path.Combine(_targetDir, _coreMvcDir, "CoreMVC", "CoreMVC.csproj");
        public static string CoreWebApiProjectPath => Path.Combine(_targetDir, _coreWebApiDir, "CoreWebApi", "CoreWebApi.csproj");
        public static string Ef6ProjectPath => Path.Combine(_targetDir, _efDir, "EF6_Test", "EF6_Test.csproj");
        public static string MvcProjectPath => Path.Combine(_targetDir, _mvcDir, "ASP.NET-MVC-Framework", "ASP.NET-MVC-Framework.csproj");
        public static string WebApiProjectPath => Path.Combine(_targetDir, _webApiDir, "WebApi-Framework", "WebApi-Framework.csproj");
        public static string WebClassLibraryProjectPath => Path.Combine(_targetDir, _webClassLibraryDir, "WebClassLibrary", "WebClassLibrary.csproj");
        public static string WindowsAuthenticationProjectPath => Path.Combine(_targetDir, _windowsAuthenticationDir, "WindowsAuthentication", "WindowsAuthentication.csproj");
        public static string FormsAuthenticationProjectPath => Path.Combine(_targetDir, _formsAuthenticationDir, _formsAuthenticationDir, "FormsAuthentication.csproj");
        public static string IISConfigProjectPath => Path.Combine(_targetDir, _iisConfigDir, "MVCAppWithIISConfig", "MVCAppWithIISConfig.csproj");
        public static string FederatedAuthenticationProjectPath => Path.Combine(_targetDir, _federatedAuthenticationDir, _federatedAuthenticationDir, "FederatedAuthentication.csproj");
        public static string CoreWCFServiceConfigProjectPath => Path.Combine(_targetDir, _wcfDir, _wcfConfigBasedProjectDir, "WCFConfigBasedProject.csproj");
        public static string CoreWCFServiceCodeProjectPath => Path.Combine(_targetDir, _wcfDir, _wcfCodeBasedProjectDir, "WCFCodeBasedProject.csproj");
        public static string VBClassLibraryProjectPath => Path.Combine(_targetDir, _vbClassLibraryDir, "VBClassLibrary", "VBClassLibrary.vbproj");
        public static string VBWebApiProjectPath => Path.Combine(_targetDir, _vbWebApiDir, "VBWebApi", "VBWebApi.vbproj");
        public static string VBNetMvcProjectPath => Path.Combine(_targetDir, _vbNetMvcDir, "VBNetMvc", "VBNetMvc.vbproj");
        public static string VBWebFormsProjectPath => Path.Combine(_targetDir, _vbWebFormsDir, "VBWebForms", "VBWebForms.vbproj");

        public static string WCFClientProjectPath => Path.Combine(_targetDir, _wcfDir, "WCFClient.csproj");
        public static string WebFormsProjectPath => Path.Combine(_targetDir, _webFormsDir, "ASP.NET-WebForms", "ASP.NET-WebForms.csproj");
        public static IEnumerable<AnalyzerResult> CoreMvcAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> CoreWebApiAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> EfAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> MvcAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> WebApiAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> WebClassLibraryAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> WebFormsAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> VBClassLibraryAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> VBWebApiAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> VBWebFormsAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> VBNetMvcAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> WindowsAuthenticationAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> IISConfigAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> FormsAuthenticationAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> FederatedAuthenticationAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> CoreWCFServiceAnalyzerResults { get; private set; }
        public static IEnumerable<AnalyzerResult> WCFClientAnalyzerResults { get; private set; }
        public static FeatureDetector FeatureDetector { get; private set; }
        public static FeatureDetectionResult CoreMvcFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult CoreWebApiFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult Ef6FeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult MvcFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult WebFormsFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult WebApiFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult WebClassLibraryFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult WindowsAuthenticationFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult FormsAuthenticationFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult IISConfigFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult FederatedAuthenticationFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult CoreWCFServiceConfigFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult CoreWCFServiceCodeFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult WCFClientFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult VBClassLibraryFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult VBWebApiFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult VBNetMvcFeatureDetectionResult { get; private set; }
        public static FeatureDetectionResult VBWebFormsFeatureDetectionResult { get; private set; }

        [OneTimeSetUp]
        public void DownloadTestProjects()
        {
            _tempDirName = Path.Combine("Projects", "TempFD");
            _tempBaseDir = Path.Combine(TestUtils.GetTstPath(), _tempDirName);
            var tempDownloadDir = Path.Combine(_tempBaseDir, "d");

            // Download test solutions
            DownloadTestProjects(tempDownloadDir);

            // Get directory of each solution
            var tempCoreMvcSolutionDir = GetSolutionDir(tempDownloadDir, CoreMvcSolutionName);
            var tempCoreWebApiSolutionDir = GetSolutionDir(tempDownloadDir, CoreWebApiSolutionName);
            var tempEfSolutionDir = GetSolutionDir(tempDownloadDir, EfSolutionName);
            var tempMvcSolutionDir = GetSolutionDir(tempDownloadDir, MvcSolutionName);
            var tempWebApiSolutionDir = GetSolutionDir(tempDownloadDir, WebApiSolutionName);
            var tempWebFormsSolutionDir = GetSolutionDir(tempDownloadDir, WebFormsSolutionName);
            var tempWebClassLibrarySolutionDir = GetSolutionDir(tempDownloadDir, WebClassLibrarySolutionName);
            var tempWindowsAuthenticationSolutionDir = GetSolutionDir(tempDownloadDir, WindowsAuthenticationSolutionName);
            var tempIISConfigSolutionDir = GetSolutionDir(tempDownloadDir, IISConfigSolutionName);
            var tempFormsAuthenticationSolutionDir = GetSolutionDir(tempDownloadDir, FormsAuthenticationSolutionName);
            var tempFederatedAuthenticationSolutionDir = GetSolutionDir(tempDownloadDir, FederatedAuthenticationSolutionName);
            var tempCoreWCFServiceSolutionDir = GetSolutionDir(tempDownloadDir, CoreWCFServiceSolutionName);
            var tempWCFClientSolutionDir = GetSolutionDir(tempDownloadDir, WCFClientSolutionName);
            var tempVBClassLibrarySolutionDir = GetSolutionDir(tempDownloadDir, VBClassLibrarySolutionName);
            var tempVBWebApiSolutionDir = GetSolutionDir(tempDownloadDir, VBWebApiSolutionName);
            var tempVBNetMvcSolutionDir = GetSolutionDir(tempDownloadDir, VBNetMvcSolutionName);
            var tempVBWebFormsSolutionDir = GetSolutionDir(tempDownloadDir, VBWebFormsSolutionName);

            // Copy solutions to a directory with a shorter path
            var destDir = "dest";
            _targetDir = Path.Combine(_tempBaseDir, destDir);
            TestUtils.CopyDirectory(tempCoreMvcSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _coreMvcDir)));
            TestUtils.CopyDirectory(tempCoreWebApiSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _coreWebApiDir)));
            TestUtils.CopyDirectory(tempEfSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _efDir)));
            TestUtils.CopyDirectory(tempMvcSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _mvcDir)));
            TestUtils.CopyDirectory(tempWebApiSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _webApiDir)));
            TestUtils.CopyDirectory(tempWebFormsSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _webFormsDir)));
            TestUtils.CopyDirectory(tempIISConfigSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _iisConfigDir)));
            TestUtils.CopyDirectory(tempWebClassLibrarySolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _webClassLibraryDir)));
            TestUtils.CopyDirectory(tempWindowsAuthenticationSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _windowsAuthenticationDir)));
            TestUtils.CopyDirectory(tempFormsAuthenticationSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _formsAuthenticationDir)));
            TestUtils.CopyDirectory(tempFederatedAuthenticationSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _federatedAuthenticationDir)));
            TestUtils.CopyDirectory(tempCoreWCFServiceSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _wcfDir)));
            TestUtils.CopyDirectory(tempWCFClientSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _wcfDir)));
            TestUtils.CopyDirectory(tempVBClassLibrarySolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _vbClassLibraryDir)));
            TestUtils.CopyDirectory(tempVBWebApiSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _vbWebApiDir)));
            TestUtils.CopyDirectory(tempVBNetMvcSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _vbNetMvcDir)));
            TestUtils.CopyDirectory(tempVBWebFormsSolutionDir, new DirectoryInfo(Path.Combine(_targetDir, _vbWebFormsDir)));

            // Run source code analysis
            VBClassLibraryAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(VBClassLibrarySolutionPath, LanguageOptions.Vb);
            VBWebApiAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(VBWebApiSolutionPath, LanguageOptions.Vb);
            VBNetMvcAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(VBNetMvcProjectPath, LanguageOptions.Vb);
            VBWebFormsAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(VBWebFormsSolutionPath, LanguageOptions.Vb);
            CoreMvcAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(CoreMvcSolutionPath);
            CoreWebApiAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(CoreWebApiSolutionPath);
            EfAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(EfSolutionPath);
            MvcAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(MvcSolutionPath);
            WebApiAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(WebApiSolutionPath);
            WebFormsAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(WebFormsSolutionPath);
            WebClassLibraryAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(WebClassLibrarySolutionPath);
            WindowsAuthenticationAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(WindowsAuthenticationSolutionPath);
            FormsAuthenticationAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(FormsAuthenticationSolutionPath);
            IISConfigAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(IISConfigSolutionPath);
            FederatedAuthenticationAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(FederatedAuthenticationSolutionPath);
            CoreWCFServiceAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(CoreWCFServiceSolutionPath);
            WCFClientAnalyzerResults = AnalyzerResultsFactory.GetAnalyzerResults(WCFClientSolutionPath);

            // Detect features in each solution
            FeatureDetector = new FeatureDetector(ConfigFile);
            VBClassLibraryFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(VBClassLibraryAnalyzerResults)[VBClassLibraryProjectPath];
            VBWebApiFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(VBWebApiAnalyzerResults)[VBWebApiProjectPath];
            VBNetMvcFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(VBNetMvcAnalyzerResults)[VBNetMvcProjectPath];
            VBWebFormsFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(VBWebFormsAnalyzerResults)[VBWebFormsProjectPath];
            CoreMvcFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(CoreMvcAnalyzerResults)[CoreMvcProjectPath];
            CoreWebApiFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(CoreWebApiAnalyzerResults)[CoreWebApiProjectPath];
            Ef6FeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(EfAnalyzerResults)[Ef6ProjectPath];
            MvcFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(MvcAnalyzerResults)[MvcProjectPath];
            WebFormsFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(WebFormsAnalyzerResults)[WebFormsProjectPath];
            WebApiFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(WebApiAnalyzerResults)[WebApiProjectPath];
            WebClassLibraryFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(WebClassLibraryAnalyzerResults)[WebClassLibraryProjectPath];
            WindowsAuthenticationFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(WindowsAuthenticationAnalyzerResults)[WindowsAuthenticationProjectPath];
            FormsAuthenticationFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(FormsAuthenticationAnalyzerResults)[FormsAuthenticationProjectPath];
            IISConfigFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(IISConfigAnalyzerResults)[IISConfigProjectPath];
            FederatedAuthenticationFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(FederatedAuthenticationAnalyzerResults)[FederatedAuthenticationProjectPath];
            CoreWCFServiceConfigFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(CoreWCFServiceAnalyzerResults)[CoreWCFServiceConfigProjectPath];
            CoreWCFServiceCodeFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(CoreWCFServiceAnalyzerResults)[CoreWCFServiceCodeProjectPath];
            WCFClientFeatureDetectionResult = FeatureDetector.DetectFeaturesInProjects(WCFClientAnalyzerResults)[WCFClientProjectPath];
        }

        [OneTimeTearDown]
        public void DeleteDownloadedProjects()
        {
            Directory.Delete(_tempBaseDir, true);
        }

        private DirectoryInfo GetSolutionDir(string startDirectory, string solutionName)
        {
            return Directory.GetParent(Directory.EnumerateFiles(startDirectory, solutionName, SearchOption.AllDirectories).FirstOrDefault());
        }

        private void DownloadTestProjects(string tempDir)
        {
            var tempDirectory = Directory.CreateDirectory(tempDir);
            var downloadLocation = Path.Combine(tempDirectory.FullName, "d");

            var fileName = Path.Combine(tempDirectory.Parent.FullName, @"TestProjects.zip");
            Rules.Config.Utils.SaveFileFromGitHub(fileName, GithubInfo.TestGithubOwner, GithubInfo.TestGithubRepo, GithubInfo.TestGithubTag);
            ZipFile.ExtractToDirectory(fileName, downloadLocation, true);
        }
    }
}