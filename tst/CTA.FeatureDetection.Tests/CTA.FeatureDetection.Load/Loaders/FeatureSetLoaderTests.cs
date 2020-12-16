using System.Linq;
using CTA.FeatureDetection.Load.Loaders;
using CTA.FeatureDetection.ProjectType.CompiledFeatures;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Load.Load
{
    public class FeatureSetLoaderTests
    {
        [Test]
        public void LoadFeatureSetFromFeatureConfig_Loads_Features_Successfully_For_Well_Defined_Config()
        {
            var featureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfig(ParsedFeatureConfigSetupFixture.WellDefinedFeatureConfig);
            Assert.True(featureSet.AllFeatures.Any());
        }

        [Test]
        public void LoadDefaultFeatureSet_Loads_All_ProjectType_Features()
        {
            var defaultFeatureSet = FeatureSetLoader.LoadDefaultFeatureSet();
            var projectTypeFeatureNames = defaultFeatureSet.CompiledFeatures.Select(f => f.Name);

            var expectedFeatureNames = new[] { nameof(AspNetMvcFeature), nameof(AspNetWebApiFeature), nameof(WebClassLibraryFeature) };

            CollectionAssert.AreEquivalent(expectedFeatureNames, projectTypeFeatureNames);
        }

        [Test]
        public void LoadFeatureSetFromType_Loads_Single_Feature_Into_FeatureSet()
        {
            var featureType = typeof(AspNetWebApiFeature);
            var projectTypeFeatureSet = FeatureSetLoader.LoadFeatureSetFromType(featureType);

            Assert.True(projectTypeFeatureSet.ConfiguredFeatures.Count == 0);
            Assert.True(projectTypeFeatureSet.CompiledFeatures.Count == 1);
            Assert.True(projectTypeFeatureSet.CompiledFeatures.First().GetType() == featureType);
        }
    }
}