using System;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Load.Loaders;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Models.Features
{
    public class FeatureSetTests : ConditionTestBase
    {
        [Test]
        public void FeatureSet_Constructor_Adds_Compiled_Feature_Collection()
        {
            var configuredFeatures = new List<ConfiguredFeature>
            {
                new ConfiguredFeature(FeatureScope.Undefined, "ConfiguredFeature1", ConditionThatIsAlwaysTrue),
                new ConfiguredFeature(FeatureScope.Undefined, "ConfiguredFeature2", ConditionThatIsAlwaysTrue)
            };
            var featureSet = new FeatureSet(configuredFeatures);

            Assert.True(featureSet.ConfiguredFeatures.Count == 2);
        }

        [Test]
        public void UnionWith_Puts_Features_In_Proper_Collections()
        {
            var validFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfig(ParsedFeatureConfigSetupFixture.WellDefinedFeatureConfig);
            var compiledFeature = validFeatureSet.CompiledFeatures.First();
            var configuredFeature = validFeatureSet.ConfiguredFeatures.First();

            var featureSetToAdd = new FeatureSet();
            featureSetToAdd.Add(compiledFeature);
            featureSetToAdd.Add(configuredFeature);

            var featureSet = new FeatureSet();
            featureSet.UnionWith(featureSetToAdd);

            Assert.True(featureSet.CompiledFeatures.First().Name == compiledFeature.Name);
            Assert.True(featureSet.ConfiguredFeatures.First().Name == configuredFeature.Name);
        }

        [Test]
        public void Add_Puts_Features_In_Proper_Collections()
        {
            var validFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfig(ParsedFeatureConfigSetupFixture.WellDefinedFeatureConfig);
            var compiledFeature = validFeatureSet.CompiledFeatures.First();
            var configuredFeature = validFeatureSet.ConfiguredFeatures.First();

            var testFeatureSet = new FeatureSet();
            testFeatureSet.Add(compiledFeature);
            testFeatureSet.Add(configuredFeature);

            Assert.True(testFeatureSet.CompiledFeatures.First().Name == compiledFeature.Name);
            Assert.True(testFeatureSet.ConfiguredFeatures.First().Name == configuredFeature.Name);
        }

        [Test]
        public void Add_Throws_Exception_If_CompiledFeature_And_ConfiguredFeature_Have_Same_Name()
        {
            var validFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfig(ParsedFeatureConfigSetupFixture.WellDefinedFeatureConfig);
            var compiledFeature = validFeatureSet.CompiledFeatures.First();
            var configuredFeature = validFeatureSet.ConfiguredFeatures.First();
            configuredFeature.Name = compiledFeature.Name;

            Assert.Throws<ArgumentException>(() =>
            {
                var testFeatureSet = new FeatureSet();
                testFeatureSet.Add(compiledFeature);
                testFeatureSet.Add(configuredFeature);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                var testFeatureSet = new FeatureSet();
                testFeatureSet.Add(configuredFeature);
                testFeatureSet.Add(compiledFeature);
            });
        }

        [Test]
        public void Add_Many_Features_Catches_Exceptions_If_Features_Have_Same_Name()
        {
            var validFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfig(ParsedFeatureConfigSetupFixture.WellDefinedFeatureConfig);
            var compiledFeature = validFeatureSet.CompiledFeatures.First();
            var configuredFeature = validFeatureSet.ConfiguredFeatures.First();
            configuredFeature.Name = compiledFeature.Name;

            var compiledFeatures = new List<CompiledFeature> {compiledFeature};

            Assert.DoesNotThrow(() =>
            {
                var testFeatureSet = new FeatureSet();
                testFeatureSet.Add(configuredFeature);
                testFeatureSet.Add(compiledFeatures);
            });
        }

        [Test]
        public void Add_Throws_Exception_When_Feature_Is_Not_Supported()
        {
            var unsupportedFeature = new UnsupportedFeature();

            Assert.Throws<NotImplementedException>(() =>
            {
                var testFeatureSet = new FeatureSet();
                testFeatureSet.Add(unsupportedFeature);
            });
        }

        private class UnsupportedFeature : Feature
        {
            public override bool IsPresent(AnalyzerResult analyzerResult)
            {
                return false;
            }
        }
    }
}