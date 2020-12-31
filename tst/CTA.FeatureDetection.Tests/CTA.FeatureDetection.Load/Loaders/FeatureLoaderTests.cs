using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CTA.FeatureDetection.Load.Loaders;
using CTA.FeatureDetection.ProjectType.CompiledFeatures;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Load.Loaders
{
    public class FeatureLoaderTests
    {
        [Test]
        public void LoadFeaturesFromNamespace_Loads_All_Features_In_Namespace()
        {
            var namespaceSuffix = nameof(global::CTA.FeatureDetection.ProjectType.CompiledFeatures);
            var assembly = Assembly.GetAssembly(typeof(AspNetMvcFeature));
            var loadedFeatures = FeatureLoader.LoadFeaturesFromNamespace(assembly, namespaceSuffix);
            var loadedFeatureNames = loadedFeatures.Select(f => f.Name);

            var expectedFeatureNames = new[]
            {
                nameof(AspNetMvcFeature), 
                nameof(AspNetWebApiFeature), 
                nameof(AspNetCoreMvcFeature), 
                nameof(AspNetCoreWebApiFeature), 
                nameof(WebClassLibraryFeature)
            };

            CollectionAssert.AreEquivalent(expectedFeatureNames, loadedFeatureNames);
        }

        [Test]
        public void LoadFeaturesFromAssemblyMetadata_Does_Not_Load_Any_Features_If_Assembly_Does_Not_Exist()
        {
            var featureDetector = new FeatureDetector(ParsedFeatureConfigSetupFixture.FeatureConfigWithNonexistentAssembly);

            Assert.False(featureDetector.LoadedFeatureSet.AllFeatures.Any());
        }

        [Test]
        public void LoadFeaturesFromAssemblyMetadata_Does_Not_Load_Feature_If_Feature_Does_Not_Exist_In_Assembly()
        {
            var featureDetector = new FeatureDetector(ParsedFeatureConfigSetupFixture.FeatureConfigWithNonexistentFeature);
            var loadedFeatureNames = featureDetector.LoadedFeatureSet.AllFeatures.Select(feature => feature.Name).ToList();

            CollectionAssert.DoesNotContain(loadedFeatureNames, "NonExistentFeature");
            CollectionAssert.Contains(loadedFeatureNames, "AspNetWebApiFeature");
        }

        [Test]
        public void LoadFeaturesFromAssemblyMetadata_Does_Not_Load_Any_Features_If_Namespace_Does_Not_Exist_In_Assembly()
        {
            var featureDetector = new FeatureDetector(ParsedFeatureConfigSetupFixture.FeatureConfigWithNonexistentNamespace);

            Assert.False(featureDetector.LoadedFeatureSet.AllFeatures.Any());
        }

        [Test]
        public void LoadFeaturesByType_Returns_Instance_Of_Specified_Type()
        {
            var featureType = typeof(AspNetMvcFeature);
            var feature = FeatureLoader.LoadFeaturesByType(featureType);

            Assert.NotNull(feature);
            Assert.True(feature.GetType() == featureType);
        }

        [Test]
        public void LoadFeaturesByTypes_Returns_Instances_Of_Specified_Types()
        {
            var featureTypes = new[] {typeof(AspNetMvcFeature), typeof(AspNetWebApiFeature)};
            var features = FeatureLoader.LoadFeaturesByTypes(featureTypes);
            var returnedFeatureTypes = features.Select(f => f.GetType());

            Assert.True(features.Count() == 2);
            CollectionAssert.Contains(returnedFeatureTypes, typeof(AspNetMvcFeature));
            CollectionAssert.Contains(returnedFeatureTypes, typeof(AspNetWebApiFeature));
        }

        [Test]
        public void LoadFeaturesFromNamespace_Returns_An_Empty_Collection_When_Parameters_Are_Invalid1()
        {
            Assembly invalidAssembly = null;
            var validNamespaceSuffix = "ProjectType";
            var loadedFeatures = FeatureLoader.LoadFeaturesFromNamespace(invalidAssembly, validNamespaceSuffix);

            CollectionAssert.IsEmpty(loadedFeatures);
        }

        [Test]
        public void LoadFeaturesFromNamespace_Returns_An_Empty_Collection_When_Parameters_Are_Invalid2()
        {
            var validAssembly = Assembly.GetAssembly(typeof(AspNetMvcFeature));
            var invalidNamespaceSuffix = "";
            var loadedFeatures = FeatureLoader.LoadFeaturesFromNamespace(validAssembly, invalidNamespaceSuffix);

            CollectionAssert.IsEmpty(loadedFeatures);
        }

        [Test]
        public void LoadFeaturesByTypes_Returns_An_Empty_Collection_When_Parameters_Are_Invalid()
        {
            var invalidTypes = new List<Type>
            {
                typeof(StringBuilder),
                typeof(Array)
            };
            var loadedFeatures = FeatureLoader.LoadFeaturesByTypes(invalidTypes);

            CollectionAssert.IsEmpty(loadedFeatures);
        }
    }
}