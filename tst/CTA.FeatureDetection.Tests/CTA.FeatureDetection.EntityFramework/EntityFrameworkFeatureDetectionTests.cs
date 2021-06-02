using CTA.FeatureDetection.Common.Models;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.EntityFramework
{
    public class EntityFrameworkFeatureDetectionTests
    {
        private const string ProjectName = "EF6_Test.csproj";
        private FeatureDetectionResult _featureDetectionResult;

        [SetUp]
        public void SetUp()
        {
            _featureDetectionResult = TestProjectsSetupFixture.Ef6FeatureDetectionResult;
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
