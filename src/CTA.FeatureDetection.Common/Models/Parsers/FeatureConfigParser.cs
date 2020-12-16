using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.Rules.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CTA.FeatureDetection.Common.Models.Parsers
{
    /// <summary>
    /// Deserializes Features' assembly metadata from json format
    /// </summary>
    public class FeatureConfigParser
    {
        private static ILogger Logger => Log.Logger;
        private static Dictionary<string, FeatureConfig> _configCache = new Dictionary<string, FeatureConfig>();

        /// <summary>
        /// Deserializes one or more feature config files
        /// </summary>
        /// <param name="configFiles">Feature config file paths</param>
        /// <returns>Deserialized FeatureGroup objects</returns>
        public static IEnumerable<FeatureConfig> Parse(IEnumerable<string> configFiles)
        {
            Logger.LogDebug($"Parsing {configFiles.Count()} feature assembly metadata file(s)...");
            var featureConfigs = configFiles.Select(Parse).Where(c => c != null);
            
            return featureConfigs;
        }

        /// <summary>
        /// Deserializes a feature config file
        /// </summary>
        /// <param name="configFile">Config file path</param>
        /// <returns>Deserialized FeatureGroup objects</returns>
        public static FeatureConfig Parse(string configFile)
        {
            if (_configCache.TryGetValue(configFile, out var featureConfig))
            {
                return featureConfig;
            }

            if (!File.Exists(configFile))
            {
                Logger.LogError($"Metadata file does not exist: {configFile}");
                return null;
            }

            Logger.LogDebug($"Parsing metadata file: {configFile}...");
            var configContent = File.ReadAllText(configFile);
            Utils.ValidateJsonObject(configContent, typeof(FeatureConfig));

            featureConfig = JsonConvert.DeserializeObject<FeatureConfig>(configContent);
            _configCache[configFile] = featureConfig;

            return featureConfig;
        }
    }
}