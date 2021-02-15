using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.ProjectType;
using CTA.FeatureDetection.ProjectType.Extensions;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.ProjectType.Extensions
{
    public class FeatureDetectionResultQueriesTests
    {
        [Test]
        public void IsNetCoreMvcProject_Returns_True_If_NetCoreMvcFeature_Is_Present()
        {
            var mockResult = new FeatureDetectionResult
            {
                FeatureStatus = { { Constants.AspNetCoreMvcFeatureName, true } }
            };

            Assert.True(mockResult.IsCoreMvcProject());
        }

        [Test]
        public void IsNetCoreWebApiProject_Returns_True_If_NetCoreWebApiFeature_Is_Present()
        {
            var mockResult = new FeatureDetectionResult
            {
                FeatureStatus = { { Constants.AspNetCoreWebApiFeatureName, true } }
            };

            Assert.True(mockResult.IsCoreWebApiProject());
        }

        [Test]
        public void IsMvcProject_Returns_True_If_MvcFeature_Is_Present()
        {
            var mockResult = new FeatureDetectionResult
            {
                FeatureStatus = { { Constants.AspNetMvcFeatureName, true } }
            };

            Assert.True(mockResult.IsMvcProject());
        }

        [Test]
        public void IsWebApiProject_Returns_True_If_WebApiFeature_Is_Present()
        {
            var mockResult = new FeatureDetectionResult
            {
                FeatureStatus = { { Constants.AspNetWebApiFeatureName, true } }
            };

            Assert.True(mockResult.IsWebApiProject());
            Assert.True(mockResult.IsWebApiProjectOnly());
        }

        [Test]
        public void IsWebApiAndMvcProject_Returns_True_If_MvcFeature_And_WebApiFeature_Are_Present()
        {
            var mockResult = new FeatureDetectionResult
            {
                FeatureStatus =
                {
                    { Constants.AspNetMvcFeatureName, true },
                    { Constants.AspNetWebApiFeatureName, true }

                }
            };

            Assert.True(mockResult.IsWebApiAndMvcProject());
        }

        [Test]
        public void IsWebClassLibrary_Returns_True_If_WebClassFeature_Is_Present()
        {
            var mockResult = new FeatureDetectionResult
            {
                FeatureStatus = { { Constants.WebClassLibraryFeatureName, true } }
            };

            Assert.True(mockResult.IsWebClassLibrary());
        }
    }
}
