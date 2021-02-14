using System;
using System.Collections.Generic;
using System.Linq;
using CTA.FeatureDetection.Common.Models.Features.Base;
using Microsoft.Extensions.Logging;

namespace CTA.FeatureDetection.Common.Models.Features
{
    /// <summary>
    /// A set of features that will bw looked for by the FeatureDetector class.
    /// These features are separated into Compiled and Configured features.
    /// </summary>
    public class FeatureSet
    {
        private const string FeatureNameConflictTemplate = "Feature with name {0} already exists in set of {1} features.";
        private ILogger Logger => Log.Logger;

        public HashSet<CompiledFeature> CompiledFeatures { get; set; }
        public HashSet<ConfiguredFeature> ConfiguredFeatures { get; set; }
        public HashSet<Feature> AllFeatures => CompiledFeatures.Concat<Feature>(ConfiguredFeatures).ToHashSet();

        public FeatureSet()
        {
            CompiledFeatures = new HashSet<CompiledFeature>();
            ConfiguredFeatures = new HashSet<ConfiguredFeature>();
        }

        public FeatureSet(IEnumerable<CompiledFeature> compiledFeatures, IEnumerable<ConfiguredFeature> configuredFeatures)
        {
            CompiledFeatures = compiledFeatures.ToHashSet();
            ConfiguredFeatures = configuredFeatures.ToHashSet();
        }

        public FeatureSet(IEnumerable<CompiledFeature> compiledFeatures)
        {
            CompiledFeatures = compiledFeatures.ToHashSet();
            ConfiguredFeatures = new HashSet<ConfiguredFeature>();
        }

        public FeatureSet(IEnumerable<ConfiguredFeature> configuredFeatures)
        {
            CompiledFeatures = new HashSet<CompiledFeature>();
            ConfiguredFeatures = configuredFeatures.ToHashSet();
        }

        public void UnionWith(FeatureSet featureSet)
        {
            Add(featureSet.ConfiguredFeatures);
            Add(featureSet.CompiledFeatures);
        }

        public void Add(IEnumerable<Feature> features)
        {
            foreach (var feature in features)
            {
                try
                {
                    Add(feature);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }
        }

        public void Add(Feature feature)
        {
            var featureType = feature.GetType();
            if (feature is CompiledFeature compiledFeature)
            {
                if (ConfiguredFeatures.Any(f => f.Name == feature.Name))
                {
                    throw new ArgumentException(string.Format(FeatureNameConflictTemplate, feature.Name, nameof(ConfiguredFeature)));
                }

                CompiledFeatures.Add(compiledFeature);
            }
            else if (feature is ConfiguredFeature configuredFeature)
            {
                if (CompiledFeatures.Any(f => f.Name == feature.Name))
                {
                    throw new ArgumentException(string.Format(FeatureNameConflictTemplate, feature.Name, nameof(CompiledFeature)));
                }

                ConfiguredFeatures.Add(configuredFeature);
            }
            else
            {
                throw new NotImplementedException($"Support for {featureType} features in {nameof(FeatureSet)} has not been implemented.");
            }
        }
    }
}
