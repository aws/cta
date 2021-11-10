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
        public void CoreWCFServiceConfigFeature_Is_Present_In_WCFConfigBasedProject()
        {
            var featureName = "CoreWCFServiceConfigFeature";
            Assert.True(_coreWCFServiceConfigFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreWCFServiceConfigProjectName} to be CoreWCFServiceConfigFeature.");
        }

        [Test]
        public void CoreWCFServiceCodeFeature_Is_Present_In_WCFConfigBasedProject()
        {
            var featureName = "CoreWCFServiceCodeFeature";
            Assert.True(_coreWCFServiceCodeFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {CoreWCFServiceCodeProjectName} to be CoreWCFServiceCodeFeature.");
        }

        [Test]
        public void WCFClientFeature_Is_Present_In_WCFClientProject()
        {
            var featureName = "WCFClientFeature";
            Assert.True(_wcfClientFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {WCFClientProjectName} to be WCFClientFeature.");
        }

        [Test]
        public void WCFServiceHostFeature_Is_Present_In_WCFConfigBasedProject()
        {
            var featureName = "WCFServiceHostFeature";
            Assert.True(_wcfServiceHostFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {WCFServiceHostProjectName} to be WCFServiceHostFeature.");
        }

        [Test]
        public void AspnetWebFormsFeature_Is_Present_In_WebForms_Project()
        {
            var featureName = "AspNetWebFormsFeature";
            Assert.True(_webFormsFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected project type of {WebFormsProjectName} to be WebForms.");
        }
    }
}