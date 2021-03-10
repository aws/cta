using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.CTA.FeatureDetection.AuthType
{
    public class AuthTypeFeatureTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void WindowsAuthenticationFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsAuthenticationFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {WindowsAuthenticationProjectName} to be present.");
        }

        [Test]
        public void WindowsAuthorizationFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsAuthorizationFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {WindowsAuthenticationProjectName} to be present.");
        }

        [Test]
        public void WindowsAuthorizationRolesFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsAuthorizationRolesFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {WindowsAuthenticationProjectName} to be present.");
        }

        [Test]
        public void WindowsImpersonationFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsImpersonationFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {WindowsAuthenticationProjectName} to be present.");
        }
    }
}