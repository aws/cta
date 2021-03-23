using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.ProjectType
{
    public class ProjectTypeFeatureDetectionTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void AspnetCoreMvcFeature_Is_Present_In_CoreMvc_Project()
        {
            var featureName = "AspNetCoreMvcFeature";
            Assert.True(_coreMvcFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreMvcProjectName} to be CoreMVC.");

            featureName = "AspNetCoreWebApiFeature";
            Assert.False(_coreMvcFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreMvcProjectName} to be CoreWebApi.");
        }

        [Test]
        public void AspnetCoreWebApiFeature_Is_Present_In_CoreWebApi_Project()
        {
            var featureName = "AspNetCoreWebApiFeature";
            Assert.True(_coreWebApiFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreWebApiProjectName} to be CoreWebApi.");

            featureName = "AspNetCoreMvcFeature";
            Assert.False(_coreWebApiFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreWebApiProjectName} to not be CoreMvc.");
        }

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

        [Test]
        public void DotnetFrameworkFeature_Is_Present_In_MvcProject()
        {
            var featureName = "DotnetFrameworkFeature";
            Assert.True(_mvcFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {MvcProjectName} to be DotnetFrameworkFeature.");
        }

        [Test]
        public void DotnetCoreFeature_Is_Present_In_CoreMvcProject()
        {
            var featureName = "DotnetCoreFeature";
            Assert.True(_coreMvcFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreMvcProjectName} to be DotnetCoreFeature.");
        }
    }
}