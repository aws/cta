﻿using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Models.Features.Conditions;
using CTA.FeatureDetection.Load.Factories;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;
using System.Linq;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Load.Factories
{
    public class ConfiguredFeatureFactoryTests : ConditionTestBase
    {
        [Test]
        public void GetInstance_Returns_Feature_With_Condition()
        {
            var featureScope = FeatureScope.Project;
            var featureName = "FeatureName";
            var feature = ConfiguredFeatureFactory.GetInstance(featureScope, featureName, ConditionMetadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature is ConfiguredFeature);
            Assert.True(feature.Condition is XmlFileQueryCondition);
        }

        [Test]
        public void GetInstance_Returns_Feature_With_Condition_Group()
        {
            var featureScope = FeatureScope.Project;
            var featureName = "FeatureName";
            var feature = ConfiguredFeatureFactory.GetInstance(featureScope, featureName, ConditionGroupMetadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature is ConfiguredFeature);
            Assert.True(feature.ConditionGroup.Conditions.All(c => c.GetType() == typeof(XmlFileQueryCondition)));
            Assert.True(feature.ConditionGroup.Conditions.Count() == 2);
        }

        [Test]
        public void GetInstance_Returns_Feature_With_Condition_Groups()
        {
            var featureScope = FeatureScope.Project;
            var featureName = "FeatureName";
            var feature = ConfiguredFeatureFactory.GetInstance(featureScope, featureName, ConditionGroupsMetadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature is ConfiguredFeature);
            Assert.True(feature.ConditionGroups.All(c => c.Conditions.Count() == 2));
            Assert.True(feature.ConditionGroups.Count() == 2);
        }
    }
}