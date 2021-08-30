using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Models.Features.Conditions;
using CTA.FeatureDetection.Load.Factories;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;
using System.Linq;
using CTA.FeatureDetection.Common.Reporting;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Load.Factories
{
    public class ConfiguredFeatureFactoryTests : ConditionTestBase
    {
        [Test]
        public void GetInstance_Returns_Feature_With_Condition()
        {
            var featureScope = FeatureScope.Project;
            var featureName = "FeatureName";
            var featureCategory = FeatureCategory.ProjectType;
            var description = "Description";
            var isLinuxCompatible = false;
            var feature = ConfiguredFeatureFactory.GetInstance(
                featureScope, 
                featureName,
                featureCategory,
                description,
                isLinuxCompatible,
                ConditionMetadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature.FeatureCategory == featureCategory);
            Assert.True(feature.Description == description);
            Assert.True(feature.IsLinuxCompatible == isLinuxCompatible);
            Assert.True(feature is ConfiguredFeature);
            Assert.True(feature.Condition is XmlFileQueryCondition);
        }

        [Test]
        public void GetInstance_Returns_Feature_With_Condition_Group()
        {
            var featureScope = FeatureScope.Project;
            var featureName = "FeatureName";
            var featureCategory = FeatureCategory.ProjectType;
            var description = "Description";
            var isLinuxCompatible = false;
            var feature = ConfiguredFeatureFactory.GetInstance(
                featureScope,
                featureName,
                featureCategory,
                description,
                isLinuxCompatible, 
                ConditionGroupMetadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature.FeatureCategory == featureCategory);
            Assert.True(feature.Description == description);
            Assert.True(feature.IsLinuxCompatible == isLinuxCompatible);
            Assert.True(feature is ConfiguredFeature);
            Assert.True(feature.ConditionGroup.Conditions.All(c => c.GetType() == typeof(XmlFileQueryCondition)));
            Assert.True(feature.ConditionGroup.Conditions.Count() == 2);
        }

        [Test]
        public void GetInstance_Returns_Feature_With_Condition_Groups()
        {
            var featureScope = FeatureScope.Project;
            var featureName = "FeatureName";
            var featureCategory = FeatureCategory.ProjectType;
            var description = "Description";
            var isLinuxCompatible = false;
            var feature = ConfiguredFeatureFactory.GetInstance(
                featureScope,
                featureName,
                featureCategory,
                description,
                isLinuxCompatible, 
                ConditionGroupsMetadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature.FeatureCategory == featureCategory);
            Assert.True(feature.Description == description);
            Assert.True(feature.IsLinuxCompatible == isLinuxCompatible);
            Assert.True(feature is ConfiguredFeature);
            Assert.True(feature.ConditionGroups.All(c => c.Conditions.Count() == 2));
            Assert.True(feature.ConditionGroups.Count() == 2);
        }
    }
}