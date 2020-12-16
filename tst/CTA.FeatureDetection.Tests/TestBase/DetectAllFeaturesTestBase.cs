using System.Collections.Generic;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Tests.Utils;
using NUnit.Framework;

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

        protected FeatureDetectionResult _ef6FeatureDetectionResult;
        protected FeatureDetectionResult _mvcFeatureDetectionResult;
        protected FeatureDetectionResult _webApiFeatureDetectionResult;
        protected FeatureDetectionResult _webClassLibraryFeatureDetectionResult;

        protected string Ef6ProjectName => "EF6_Test";
        protected string MvcProjectName => "ASP.NET-MVC-Framework";
        protected string WebApiProjectName => "WebApi-Framework";
        protected string WebClassLibraryProjectName => "WebClassLibrary";

        [SetUp]
        public void SetUp()
        {
            TestProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));
            FeatureDetector = TestProjectsSetupFixture.FeatureDetector;
            _ef6FeatureDetectionResult = TestProjectsSetupFixture.Ef6FeatureDetectionResult;
            _mvcFeatureDetectionResult = TestProjectsSetupFixture.MvcFeatureDetectionResult;
            _webApiFeatureDetectionResult = TestProjectsSetupFixture.WebApiFeatureDetectionResult;
            _webClassLibraryFeatureDetectionResult = TestProjectsSetupFixture.WebClassLibraryFeatureDetectionResult;
        }
    }
}