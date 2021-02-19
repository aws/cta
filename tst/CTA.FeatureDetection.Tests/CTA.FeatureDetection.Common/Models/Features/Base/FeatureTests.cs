using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.ProjectType.CompiledFeatures;
using NUnit.Framework;
using System.Linq;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Models.Features.Base
{
    public class FeatureTests
    {
        [Test]
        public void Instantiating_An_Invalid_Feature_Does_Not_Load_It()
        {
            var featureDetector = new FeatureDetector(ParsedFeatureConfigSetupFixture.FeatureConfigWithInvalidFeature);

            Assert.False(featureDetector.LoadedFeatureSet.AllFeatures.Any());
        }

        [Test]
        public void Features_Are_Equal_If_Their_Names_Match()
        {
            Feature feature1 = new AspNetMvcFeature
            {
                Name = "FeatureName"
            };
            Feature feature2 = new AspNetWebApiFeature
            {
                Name = "FeatureName"
            };

            Assert.True(feature1.Equals(feature2));
        }
    }
}
