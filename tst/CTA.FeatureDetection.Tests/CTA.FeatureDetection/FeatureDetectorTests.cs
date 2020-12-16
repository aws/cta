using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.FeatureDetection.Load.Loaders;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection
{
    public class FeatureDetectorTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void FeatureDetector_Default_Constructor_Loads_All_ProjectType_Features()
        {
            var defaultFeatureDetector = new FeatureDetector();
            var defaultFeatureSet = FeatureSetLoader.LoadDefaultFeatureSet();

            var loadedFeatureNames = defaultFeatureDetector.LoadedFeatureSet.CompiledFeatures.Select(f => f.Name);
            var defaultFeatureNames = defaultFeatureSet.CompiledFeatures.Select(f => f.Name);

            var difference = defaultFeatureNames.Except(loadedFeatureNames);

            Assert.IsNotEmpty(defaultFeatureNames);
            Assert.IsNotEmpty(loadedFeatureNames);
            Assert.IsEmpty(difference);
        }

        [Test]
        public void FeatureDetector_Default_Constructor_Catches_Exceptions()
        {
            var featureConfig = ParsedFeatureConfigSetupFixture.FeatureConfigWithNonexistentAssembly;
            Assert.DoesNotThrow(() => new FeatureDetector(featureConfig));
        }

        [Test]
        public void FeatureDetector_Loads_Features_In_Specified_Config()
        {
            var featureConfig = Path.Combine(TestProjectDirectory, "Examples", "Input", "EntityFramework_config_features.json");
            var featureDetector = new FeatureDetector(featureConfig);
            var loadedFeatureSet = featureDetector.LoadedFeatureSet;
            
            Assert.IsEmpty(loadedFeatureSet.CompiledFeatures);
            Assert.True(loadedFeatureSet.ConfiguredFeatures.Count == 9);
        }

        [Test]
        public void FeatureDetector_Loads_Features_In_Specified_Configs()
        {
            var featureConfig1 = Path.Combine(TestProjectDirectory, "Examples", "Input", "EntityFramework_config_features.json");
            var featureConfig2 = Path.Combine(TestProjectDirectory, "Examples", "Input", "ProjectType_code_features.json");
            var featureConfigs = new List<string>
            {
                featureConfig1,
                featureConfig2
            };
            var featureDetector = new FeatureDetector(featureConfigs);
            var loadedFeatureSet = featureDetector.LoadedFeatureSet;
            
            CollectionAssert.IsNotEmpty(loadedFeatureSet.ConfiguredFeatures);
            CollectionAssert.IsNotEmpty(loadedFeatureSet.CompiledFeatures);
            Assert.True(loadedFeatureSet.ConfiguredFeatures.Count == 9);
            Assert.True(loadedFeatureSet.CompiledFeatures.Count == 3);
        }

        [Test]
        public void DetectFeaturesInProject_Detects_Features_In_Specified_Project()
        {
            const string mvcFeature = "AspNetMvcFeature";
            const string webApiFeature = "AspNetWebApiFeature";
            const string sqlServerProviderFeature = "SqlServerProviderFeature";
            const string postgresProviderFeature = "PostgresProviderFeature";

            var results = FeatureDetector.DetectFeaturesInProject(TestProjectsSetupFixture.MvcProjectPath);

            Assert.True(results.FeatureStatus[mvcFeature]);
            Assert.False(results.FeatureStatus[webApiFeature]);
            Assert.False(results.FeatureStatus[sqlServerProviderFeature]);
            Assert.False(results.FeatureStatus[postgresProviderFeature]);
        }

        [Test]
        public void DetectFeaturesInSolution_Detects_Features_In_Specified_Solution()
        {
            const string mvcFeature = "AspNetMvcFeature";
            const string webApiFeature = "AspNetWebApiFeature";
            const string sqlServerProviderFeature = "SqlServerProviderFeature";
            const string postgresProviderFeature = "PostgresProviderFeature";

            var results = FeatureDetector.DetectFeaturesInSolution(TestProjectsSetupFixture.MvcSolutionPath);

            var mvcResults = results[TestProjectsSetupFixture.MvcProjectPath];
            Assert.True(mvcResults.FeatureStatus[mvcFeature]);
            Assert.False(mvcResults.FeatureStatus[webApiFeature]);
            Assert.False(mvcResults.FeatureStatus[sqlServerProviderFeature]);
            Assert.False(mvcResults.FeatureStatus[postgresProviderFeature]);
        }
    }
}