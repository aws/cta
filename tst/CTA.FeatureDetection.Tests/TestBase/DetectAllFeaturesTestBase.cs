using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Tests.Utils;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.FeatureDetection.Tests.TestBase
{
    /// <summary>
    /// Detects all features in each of the projects that belong to TestProjectsSetupFixture.SolutionPath
    /// </summary>
    public class DetectAllFeaturesTestBase
    {
        protected static IEnumerable<AnalyzerResult> EfAnalyzerResults => TestProjectsSetupFixture.EfAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> MvcAnalyzerResults => TestProjectsSetupFixture.MvcAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> WebFormsAnalyzerResults => TestProjectsSetupFixture.WebFormsAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> WebApiAnalyzerResults => TestProjectsSetupFixture.WebApiAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> WebClassLibraryAnalyzerResults => TestProjectsSetupFixture.WebClassLibraryAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> VBClassLibraryAnalyzerResults => TestProjectsSetupFixture.VBClassLibraryAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> VBWebApiAnalyzerResults => TestProjectsSetupFixture.VBWebApiAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> VBNetMvcAnalyzerResults => TestProjectsSetupFixture.VBNetMvcAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> VBWebFormsAnalyzerResults => TestProjectsSetupFixture.VBWebFormsAnalyzerResults;
        protected static FeatureDetector FeatureDetector { get; private set; }
        protected static string TestProjectDirectory { get; private set; }

        protected FeatureDetectionResult _coreMvcFeatureDetectionResult;
        protected FeatureDetectionResult _coreWebApiFeatureDetectionResult;
        protected FeatureDetectionResult _ef6FeatureDetectionResult;
        protected FeatureDetectionResult _mvcFeatureDetectionResult;
        protected FeatureDetectionResult _webFormsFeatureDetectionResult;
        protected FeatureDetectionResult _webApiFeatureDetectionResult;
        protected FeatureDetectionResult _webClassLibraryFeatureDetectionResult;
        protected FeatureDetectionResult _windowsAuthenticationFeatureDetectionResult;
        protected FeatureDetectionResult _formsAuthenticationFeatureDetectionResult;
        protected FeatureDetectionResult _federatedAuthenticationFeatureDetectionResult;
        protected FeatureDetectionResult _iisConfigFeatureDetectionResult;
        protected FeatureDetectionResult _coreWCFServiceConfigFeatureDetectionResult;
        protected FeatureDetectionResult _coreWCFServiceCodeFeatureDetectionResult;
        protected FeatureDetectionResult _wcfClientFeatureDetectionResult;
        protected FeatureDetectionResult _wcfServiceHostFeatureDetectionResult;
        protected FeatureDetectionResult _VBClassLibraryFeatureDetectionResult;
        protected FeatureDetectionResult _VBWebApiFeatureDetectionResult;
        protected FeatureDetectionResult _VBNetMvcFeatureDetectionResult;
        protected FeatureDetectionResult _VBWebFormsFeatureDetectionResult;
        protected string CoreMvcProjectName => "CoreMVC";
        protected string CoreWebApiProjectName => "CoreWebApi";
        protected string MvcProjectName => "ASP.NET-MVC-Framework";
        protected string WebFormsProjectName => "ASP.NET-WebForms";
        protected string WebApiProjectName => "WebApi-Framework";
        protected string WebClassLibraryProjectName => "WebClassLibrary";
        protected string CoreWCFServiceConfigProjectName => "WCFConfigBasedProject";
        protected string CoreWCFServiceCodeProjectName => "WCFCodeBasedProject";
        protected string WCFClientProjectName => "WCFClientProject";
        protected string WCFServiceHostProjectName => "WCFServiceHostProject";
        protected string VBClassLibraryProjectName => "VBClassLibrary";
        protected string VBWebApiProjectName => "VBWebApi";
        protected string VBNetMvcProjectName => "VBNetMvc";
        protected string VBWebFormsProjectName => "VBWebForms";

        [SetUp]
        public void SetUp()
        {
            TestProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));
            FeatureDetector = TestProjectsSetupFixture.FeatureDetector;
            _coreMvcFeatureDetectionResult = TestProjectsSetupFixture.CoreMvcFeatureDetectionResult;
            _coreWebApiFeatureDetectionResult = TestProjectsSetupFixture.CoreWebApiFeatureDetectionResult;
            _ef6FeatureDetectionResult = TestProjectsSetupFixture.Ef6FeatureDetectionResult;
            _mvcFeatureDetectionResult = TestProjectsSetupFixture.MvcFeatureDetectionResult;
            _webFormsFeatureDetectionResult = TestProjectsSetupFixture.WebFormsFeatureDetectionResult;
            _webApiFeatureDetectionResult = TestProjectsSetupFixture.WebApiFeatureDetectionResult;
            _webClassLibraryFeatureDetectionResult = TestProjectsSetupFixture.WebClassLibraryFeatureDetectionResult;
            _windowsAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.WindowsAuthenticationFeatureDetectionResult;
            _formsAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.FormsAuthenticationFeatureDetectionResult;
            _iisConfigFeatureDetectionResult = TestProjectsSetupFixture.IISConfigFeatureDetectionResult;
            _federatedAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.FederatedAuthenticationFeatureDetectionResult;
            _coreWCFServiceConfigFeatureDetectionResult = TestProjectsSetupFixture.CoreWCFServiceConfigFeatureDetectionResult;
            _coreWCFServiceCodeFeatureDetectionResult = TestProjectsSetupFixture.CoreWCFServiceCodeFeatureDetectionResult;
            _wcfClientFeatureDetectionResult = TestProjectsSetupFixture.WCFClientFeatureDetectionResult;
            _wcfServiceHostFeatureDetectionResult = TestProjectsSetupFixture.CoreWCFServiceCodeFeatureDetectionResult;
            _VBClassLibraryFeatureDetectionResult = TestProjectsSetupFixture.VBClassLibraryFeatureDetectionResult;
            _VBWebApiFeatureDetectionResult= TestProjectsSetupFixture.VBWebApiFeatureDetectionResult;
            _VBNetMvcFeatureDetectionResult = TestProjectsSetupFixture.VBNetMvcFeatureDetectionResult;
            _VBWebFormsFeatureDetectionResult = TestProjectsSetupFixture.VBWebFormsFeatureDetectionResult;
        }
    }
}