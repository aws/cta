using CTA.FeatureDetection.Common.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class FeatureGroup
    {
        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FeatureScope FeatureScope { get; set; }

        public CompiledFeatureAssembly[] CompiledFeatureAssemblies { get; set; }

        public ConfiguredFeatureMetadata[] ConfiguredFeatures { get; set; }
    }
}