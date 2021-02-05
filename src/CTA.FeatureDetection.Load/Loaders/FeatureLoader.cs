using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CTA.FeatureDetection.Common;
using CTA.FeatureDetection.Common.Exceptions;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Load.Factories;
using Microsoft.Extensions.Logging;

namespace CTA.FeatureDetection.Load.Loaders
{
    /// <summary>
    /// Loads features into memory from features metadata objects
    /// </summary>
    public class FeatureLoader
    {
        private static ILogger Logger => Log.Logger;

        /// <summary>
        /// Loads compiled features from an assembly specified in a metadata collection
        /// </summary>
        /// <param name="featureScope">The scope in which to look for the features</param>
        /// <param name="compiledFeatureAssemblies">Collection of feature assembly metadata</param>
        /// <returns>Collection of compiled feature instances</returns>
        public static HashSet<CompiledFeature> LoadCompiledFeaturesFromAssemblies(
            FeatureScope featureScope,
            CompiledFeatureAssembly[] compiledFeatureAssemblies)
        {
            var loadedFeatures = new HashSet<CompiledFeature>();
            foreach (var compiledFeatureAssembly in compiledFeatureAssemblies)
            {
                try
                {
                    var compiledFeatures = LoadCompiledFeaturesFromAssembly(featureScope, compiledFeatureAssembly);
                    loadedFeatures.UnionWith(compiledFeatures);
                }
                catch (FileNotFoundException ex)
                {
                    Logger.LogError(ex, $"Could not find assembly {compiledFeatureAssembly.AssemblyPath}");
                }
            }

            return loadedFeatures;
        }

        /// <summary>
        /// Loads configured features using metadata
        /// </summary>
        /// <param name="featureScope">The scope in which to look for the features</param>
        /// <param name="configuredFeatures">Collection of configured feature metadata</param>
        /// <returns>Collection of configured feature instances</returns>
        public static HashSet<ConfiguredFeature> LoadConfiguredFeatures(FeatureScope featureScope, ConfiguredFeatureMetadata[] configuredFeatures)
        {
            var loadedConfiguredFeatures = new HashSet<ConfiguredFeature>();

            foreach (var configuredFeature in configuredFeatures)
            {
                try
                {
                    if (configuredFeature.ConditionGroups != null && configuredFeature.ConditionGroups.Any())
                    {
                        var loadedFeature = ConfiguredFeatureFactory.GetInstance(featureScope, configuredFeature.Name, configuredFeature.ConditionGroups);
                        loadedConfiguredFeatures.Add(loadedFeature);
                    }
                    else if (configuredFeature.ConditionGroup != null)
                    {
                        var loadedFeature = ConfiguredFeatureFactory.GetInstance(featureScope, configuredFeature.Name, configuredFeature.ConditionGroup);
                        loadedConfiguredFeatures.Add(loadedFeature);
                    }
                    else if (configuredFeature.Condition != null)
                    {
                        var loadedFeature = ConfiguredFeatureFactory.GetInstance(featureScope, configuredFeature.Name, configuredFeature.Condition);
                        loadedConfiguredFeatures.Add(loadedFeature);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.LogError(ex, $"Error encountered when trying to load ConfiguredFeature {configuredFeature.Name}.");
                    Console.WriteLine(ex);
                }
            }

            return loadedConfiguredFeatures;
        }

        /// <summary>
        /// Dynamically instantiates all features in a given namespaceSuffix
        /// </summary>
        /// <param name="assembly">Assembly containing the feature types</param>
        /// <param name="namespaceSuffix">Namespace containing the feature types</param>
        /// <returns>Instances of all features in the namespace with the given suffix</returns>
        public static IEnumerable<CompiledFeature> LoadFeaturesFromNamespace(Assembly assembly, string namespaceSuffix)
        {
            var loadedFeatures = new List<CompiledFeature>();
            if (assembly == null || string.IsNullOrEmpty(namespaceSuffix))
            {
                return loadedFeatures;
            }

            var featureScope = FeatureScope.Undefined;
            var featureTypes = assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.EndsWith(namespaceSuffix));
            foreach (var featureType in featureTypes)
            {
                var featureMetadata = new CompiledFeatureMetadata
                {
                    Name = featureType.Name,
                    ClassName = featureType.Name
                };

                try
                {
                    var loadedFeature = CompiledFeatureFactory.GetInstance(featureScope, assembly, featureType.Namespace, featureMetadata);
                    loadedFeatures.Add(loadedFeature);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            return loadedFeatures;
        }

        /// <summary>
        /// Dynamically instantiates all features in a given collection of CompiledFeature types
        /// </summary>
        /// <param name="types">CompiledFeature types to instantiate</param>
        /// <returns>CompiledFeature instances</returns>
        public static IEnumerable<CompiledFeature> LoadFeaturesByTypes(IEnumerable<Type> types)
        {
            var loadedFeatures = new List<CompiledFeature>();
            foreach (var type in types)
            {
                try
                {
                    var loadedFeature = LoadFeaturesByType(type);
                    loadedFeatures.Add(loadedFeature);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            return loadedFeatures;
        }

        /// <summary>
        /// Dynamically instantiates a CompiledFeature by its type
        /// </summary>
        /// <param name="type">CompiledFeature type to instantiate</param>
        /// <returns>CompiledFeature instance</returns>
        public static CompiledFeature LoadFeaturesByType(Type type)
        {
            return CompiledFeatureFactory.GetInstance(type);
        }

        private static HashSet<CompiledFeature> LoadCompiledFeaturesFromAssembly(FeatureScope featureScope, CompiledFeatureAssembly compiledFeatureAssembly)
        {
            var loadedFeatures = new HashSet<CompiledFeature>();

            var loadedAssembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), compiledFeatureAssembly.AssemblyPath));
            foreach (var featureNamespace in compiledFeatureAssembly.CompiledFeatureNamespaces)
            {
                var compiledFeatures = LoadCompiledFeaturesByNamespace(featureScope, loadedAssembly, featureNamespace);
                loadedFeatures.UnionWith(compiledFeatures);
            }

            return loadedFeatures;
        }

        private static HashSet<CompiledFeature> LoadCompiledFeaturesByNamespace(
            FeatureScope featureScope,
            Assembly assembly,
            CompiledFeatureNamespace featureNamespace)
        {
            var loadedFeatures = new HashSet<CompiledFeature>();
            foreach (var featureMetadata in featureNamespace.CompiledFeatureMetadata)
            {
                try
                {
                    var compiledFeature = CompiledFeatureFactory.GetInstance(featureScope, assembly, featureNamespace.Namespace, featureMetadata);
                    loadedFeatures.Add(compiledFeature);
                }
                catch (ClassNotFoundException e)
                {
                    Logger.LogError(e, e.Message);
                }
                catch (InvalidFeatureException e)
                {
                    Logger.LogError(e, e.Message);
                }
            }

            return loadedFeatures;
        }
    }
}
