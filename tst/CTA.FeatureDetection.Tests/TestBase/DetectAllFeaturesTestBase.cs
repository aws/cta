using Codelyzer.Analysis;
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
        protected FeatureDetectionResult _coreWCFServiceConfigFeatureDetectionResult;
        protected FeatureDetectionResult _coreWCFServiceCodeFeatureDetectionResult;
        protected FeatureDetectionResult _wcfClientFeatureDetectionResult;
        protected FeatureDetectionResult _wcfServiceHostFeatureDetectionResult;

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
            _federatedAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.FederatedAuthenticationFeatureDetectionResult;
            _coreWCFServiceConfigFeatureDetectionResult = TestProjectsSetupFixture.CoreWCFServiceConfigFeatureDetectionResult;
            _coreWCFServiceCodeFeatureDetectionResult = TestProjectsSetupFixture.CoreWCFServiceCodeFeatureDetectionResult;
            _wcfClientFeatureDetectionResult = TestProjectsSetupFixture.WCFClientFeatureDetectionResult;
            _wcfServiceHostFeatureDetectionResult = TestProjectsSetupFixture.CoreWCFServiceCodeFeatureDetectionResult;
        }
    }
}