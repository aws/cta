using System.Linq;
using CTA.FeatureDetection.AuthType.CompiledFeatures;
using CTA.FeatureDetection.Tests.TestBase;
using CTA.FeatureDetection.Tests.Utils;
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
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void WindowsAuthorizationFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsAuthorizationFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void WindowsAuthorizationRolesFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsAuthorizationRolesFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void WindowsImpersonationFeature_Is_Present_In_WindowsAuthentication_Project()
        {
            var featureName = "WindowsImpersonationFeature";
            Assert.True(_windowsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void FormsAuthenticationFeature_Is_Present_In_FormsAuthentication_Project()
        {
            var featureName = "FormsAuthenticationFeature";
            Assert.True(_formsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void FormsAuthenticationWithMembershipFeature_Is_Present_In_FormsAuthentication_Project()
        {
            var featureName = "FormsAuthenticationWithMembershipFeature";
            Assert.True(_formsAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void FederatedAuthenticationFeature_Is_Present_In_FormsAuthentication_Project()
        {
            var featureName = "FederatedAuthenticationFeature";
            Assert.True(_federatedAuthenticationFeatureDetectionResult.FeatureStatus[featureName],
                $"Expected authentication type of {featureName} to be present.");
        }

        [Test]
        public void IsAuthorizeRoleAttributeInCode_Returns_True_When_Attribute_Exists_In_Code()
        {
            var windowsAuthorizationRolesFeature = new WindowsAuthorizationRolesFeature();
            var method = TestUtils.GetPrivateMethod(windowsAuthorizationRolesFeature.GetType(), "IsAuthorizeRoleAttributeInCode");

            var analyzerResult = TestProjectsSetupFixture.FormsAuthenticationAnalyzerResults.First();
            var isAuthorizeRoleAttributeInCode = (bool?)method.Invoke(windowsAuthorizationRolesFeature, new object []{ analyzerResult });
            
            Assert.True(isAuthorizeRoleAttributeInCode, "Expected Authorize Role attribute to be present.");
        }
    }
}