using CTA.FeatureDetection.Common.Exceptions;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Load.Factories;
using CTA.FeatureDetection.ProjectType.CompiledFeatures;
using NUnit.Framework;
using System.Reflection;
using System.Text;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Load.Factories
{
    public class CompiledFeatureFactoryTests
    {
        [Test]
        public void GetInstance_Returns_Feature_From_Assembly()
        {
            var featureType = typeof(AspNetMvcFeature);
            var assembly = Assembly.GetAssembly(featureType);
            var featureScope = FeatureScope.Project;
            var @namespace = featureType.Namespace;

            var featureName = "FeatureName";
            var className = nameof(AspNetMvcFeature);
            var metadata = new CompiledFeatureMetadata
            {
                Name = featureName,
                ClassName = className
            };

            var feature = CompiledFeatureFactory.GetInstance(featureScope, assembly, @namespace, metadata);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature is AspNetMvcFeature);
        }

        [Test]
        public void GetInstance_Returns_Feature_From_Type_With_Attributes()
        {
            var featureType = typeof(AspNetMvcFeature);
            var featureName = "FeatureName";
            var featureScope = FeatureScope.Project;
            var feature = CompiledFeatureFactory.GetInstance(featureType, featureName, featureScope);

            Assert.NotNull(feature);
            Assert.True(feature.Name == featureName);
            Assert.True(feature.FeatureScope == featureScope);
            Assert.True(feature is AspNetMvcFeature);
        }

        [Test]
        public void GetInstance_Returns_Feature_From_Type()
        {
            var featureType = typeof(AspNetMvcFeature);
            var feature = CompiledFeatureFactory.GetInstance(featureType);

            Assert.NotNull(feature);
            Assert.True(feature is AspNetMvcFeature);
        }

        [Test]
        public void GetInstance_Throws_InvalidFeatureException_If_Type_Is_Not_A_Feature1()
        {
            var notAFeatureType = typeof(StringBuilder);
            Assert.Throws<InvalidFeatureException>(() => CompiledFeatureFactory.GetInstance(notAFeatureType));
        }

        [Test]
        public void GetInstance_Throws_InvalidFeatureException_If_Type_Is_Not_A_Feature2()
        {
            var notAFeatureType = typeof(StringBuilder);
            var featureName = "SomeName";
            var featureScope = FeatureScope.Undefined;
            Assert.Throws<InvalidFeatureException>(() => CompiledFeatureFactory.GetInstance(notAFeatureType, featureName, featureScope));
        }

        [Test]
        public void GetInstance_Throws_InvalidFeatureException_If_Type_Is_Not_A_Feature3()
        {
            var notAFeatureType = typeof(StringBuilder);
            var featureMetadata = new CompiledFeatureMetadata
            {
                Name = "SomeName",
                ClassName = notAFeatureType.Name
            };
            var featureScope = FeatureScope.Undefined;
            var assembly = Assembly.GetAssembly(notAFeatureType);
            var @namespace = notAFeatureType.Namespace;
            Assert.Throws<InvalidFeatureException>(() => CompiledFeatureFactory.GetInstance(featureScope, assembly, @namespace, featureMetadata));
        }
    }
}