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
        protected static IEnumerable<AnalyzerResult> WebApiAnalyzerResults => TestProjectsSetupFixture.WebApiAnalyzerResults;
        protected static IEnumerable<AnalyzerResult> WebClassLibraryAnalyzerResults => TestProjectsSetupFixture.WebClassLibraryAnalyzerResults;
        protected static FeatureDetector FeatureDetector { get; private set; }
        protected static string TestProjectDirectory { get; private set; }

        protected FeatureDetectionResult _coreMvcFeatureDetectionResult;
        protected FeatureDetectionResult _coreWebApiFeatureDetectionResult;
        protected FeatureDetectionResult _ef6FeatureDetectionResult;
        protected FeatureDetectionResult _mvcFeatureDetectionResult;
        protected FeatureDetectionResult _webApiFeatureDetectionResult;
        protected FeatureDetectionResult _webClassLibraryFeatureDetectionResult;
        protected FeatureDetectionResult _windowsAuthenticationFeatureDetectionResult;
        protected FeatureDetectionResult _formsAuthenticationFeatureDetectionResult;
        protected FeatureDetectionResult _federatedAuthenticationFeatureDetectionResult;

        protected string CoreMvcProjectName => "CoreMVC";
        protected string CoreWebApiProjectName => "CoreWebApi";
        protected string MvcProjectName => "ASP.NET-MVC-Framework";
        protected string WebApiProjectName => "WebApi-Framework";
        protected string WebClassLibraryProjectName => "WebClassLibrary";

        [SetUp]
        public void SetUp()
        {
            TestProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));
            FeatureDetector = TestProjectsSetupFixture.FeatureDetector;
            _coreMvcFeatureDetectionResult = TestProjectsSetupFixture.CoreMvcFeatureDetectionResult;
            _coreWebApiFeatureDetectionResult = TestProjectsSetupFixture.CoreWebApiFeatureDetectionResult;
            _ef6FeatureDetectionResult = TestProjectsSetupFixture.Ef6FeatureDetectionResult;
            _mvcFeatureDetectionResult = TestProjectsSetupFixture.MvcFeatureDetectionResult;
            _webApiFeatureDetectionResult = TestProjectsSetupFixture.WebApiFeatureDetectionResult;
            _webClassLibraryFeatureDetectionResult = TestProjectsSetupFixture.WebClassLibraryFeatureDetectionResult;
            _windowsAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.WindowsAuthenticationFeatureDetectionResult;
            _formsAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.FormsAuthenticationFeatureDetectionResult;
            _federatedAuthenticationFeatureDetectionResult = TestProjectsSetupFixture.FederatedAuthenticationFeatureDetectionResult;
        }
    }
}