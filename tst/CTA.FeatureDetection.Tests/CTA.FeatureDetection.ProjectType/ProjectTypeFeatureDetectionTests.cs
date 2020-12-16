using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.ProjectType
{
    public class ProjectTypeFeatureDetectionTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void AspnetMvcFeature_Is_Present_In_Mvc_Project()
        {
            var featureName = "AspNetMvcFeature";
            Assert.True(_mvcFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {MvcProjectName} to be MVC.");
        }

        [Test]
        public void AspnetWebApiFeature_Is_Present_In_WebApi_Project()
        {
            var featureName = "AspNetWebApiFeature";
            Assert.True(_webApiFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {WebApiProjectName} to be WebApi.");
        }

        [Test]
        public void SystemWeb_Is_Present_In_WebClassLibrary()
        {
            var featureName = "WebClassLibraryFeature";
            Assert.True(_webClassLibraryFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {WebClassLibraryProjectName} to be WebClassLibrary.");
        }
    }
}