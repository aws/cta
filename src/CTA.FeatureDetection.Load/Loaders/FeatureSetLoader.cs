using System;
using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Features;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Models.Parsers;

namespace CTA.FeatureDetection.Load.Loaders
{
    public class FeatureSetLoader
    {
        /// <summary>
        /// Loads features to be detected into a FeatureSet object using metadata stored in FeatureConfig objects
        /// </summary>
        /// <param name="featureConfigs">Collection of FeatureConfig objects</param>
        /// <returns>FeatureSet object containing all features loaded into memory</returns>
        public static FeatureSet LoadFeatureSetFromFeatureConfigs(IEnumerable<FeatureConfig> featureConfigs)
        {
            var featureSet = new FeatureSet();
            foreach (var featureConfig in featureConfigs)
            {
                var featureSubset = LoadFeatureSetFromFeatureConfig(featureConfig);
                featureSet.UnionWith(featureSubset);
            }

            return featureSet;
        }

        /// <summary>
        /// Load features to be detected into a FeatureSet object from config file
        /// </summary>
        /// <param name="featureConfigPath">Collection of config file path</param>
        /// <returns>FeatureSet containing features specified in config file</returns>
        public static FeatureSet LoadFeatureSetFromFeatureConfigFile(string featureConfigPath)
        {
            return LoadFeatureSetFromFeatureConfigFiles(new[] { featureConfigPath });
        }

        /// <summary>
        /// Load features to be detected into a FeatureSet object from config files
        /// </summary>
        /// <param name="featureConfigPaths">Collection of config file paths</param>
        /// <returns>FeatureSet containing features specified in config files</returns>
        public static FeatureSet LoadFeatureSetFromFeatureConfigFiles(IEnumerable<string> featureConfigPaths)
        {
            var featureConfigs = FeatureConfigParser.Parse(featureConfigPaths);
            return LoadFeatureSetFromFeatureConfigs(featureConfigs);
        }

        /// <summary>
        /// Loads features to be detected into a FeatureSet object using metadata stored in a single FeatureConfig object
        /// </summary>
        /// <param name="featureConfig">FeatureConfig object</param>
        /// <returns>FeatureSet object containing all features loaded into memory</returns>
        public static FeatureSet LoadFeatureSetFromFeatureConfig(FeatureConfig featureConfig)
        {
            var compiledFeatures = new HashSet<CompiledFeature>();
            var configuredFeatures = new HashSet<ConfiguredFeature>();
            foreach (var featureGroup in featureConfig.FeatureGroups)
            {
                var featureScope = featureGroup.FeatureScope;
                var newCompiledFeatures = FeatureLoader.LoadCompiledFeaturesFromAssemblies(featureScope, featureGroup.CompiledFeatureAssemblies);
                var newConfiguredFeatures = FeatureLoader.LoadConfiguredFeatures(featureScope, featureGroup.ConfiguredFeatures);

                compiledFeatures.UnionWith(newCompiledFeatures);
                configuredFeatures.UnionWith(newConfiguredFeatures);
            }

            return new FeatureSet(compiledFeatures, configuredFeatures);
        }

        /// <summary>
        /// Create a FeatureSet using a feature type
        /// </summary>
        /// <param name="type">Type of feature to include</param>
        /// <returns>FeatureSet containing the feature of the specified type</returns>
        public static FeatureSet LoadFeatureSetFromType(Type type)
        {
            return LoadFeatureSetFromTypes(new[] { type });
        }

        /// <summary>
        /// Create a FeatureSet using feature types
        /// </summary>
        /// <param name="types">Types of features to include</param>
        /// <returns>FeatureSet containing the features of the specified types</returns>
        public static FeatureSet LoadFeatureSetFromTypes(IEnumerable<Type> types)
        {
            var features = FeatureLoader.LoadFeaturesByTypes(types);
            return new FeatureSet(features);
        }

        /// <summary>
        /// Load the FeatureSet defined in the default feature config file
        /// </summary>
        /// <returns>Default FeatureSet</returns>
        public static FeatureSet LoadDefaultFeatureSet()
        {
            return LoadFeatureSetFromFeatureConfigFile(Constants.DefaultFeatureConfigPath);
        }
    }
}