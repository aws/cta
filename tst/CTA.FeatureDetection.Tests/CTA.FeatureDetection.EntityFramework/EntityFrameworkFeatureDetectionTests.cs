using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Tests.Utils;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace CTA.FeatureDetection.Tests.FeatureDetection.EntityFramework
{
    public class EntityFrameworkFeatureDetectionTests
    {
        private const string ProjectName = "EF6_Test.csproj";
        private static readonly string TestProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));
        private static string ConfigFile => Path.Combine(TestProjectDirectory, "Examples", "Templates", "EntityFramework_config_features.json");
        private FeatureDetectionResult _featureDetectionResult;

        [SetUp]
        public void SetUp()
        {
            var analyzerResult = TestProjectsSetupFixture.EfAnalyzerResults.First(a =>
                a.ProjectResult.ProjectFilePath == TestProjectsSetupFixture.Ef6ProjectPath);

            var featureDetector = new FeatureDetector(ConfigFile);
            _featureDetectionResult = featureDetector.DetectFeaturesInProject(analyzerResult);
        }

        [Test]
        public void Multiple_Providers_Are_Present_In_App_Config()
        {
            var sqlServerProviderFeatureName = "SqlServerProviderFeature";
            var postgresProviderFeatureName = "PostgresProviderFeature";

            Assert.True(_featureDetectionResult.FeatureStatus[sqlServerProviderFeatureName],
                $"Expected {sqlServerProviderFeatureName} to be present in {ProjectName}.");

            Assert.True(_featureDetectionResult.FeatureStatus[postgresProviderFeatureName],
                $"Expected {postgresProviderFeatureName} to be present in {ProjectName}.");
        }
    }
}
