using Newtonsoft.Json;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class CompiledFeatureNamespace
    {
        [JsonProperty(Required = Required.Always)]
        public string Namespace { get; set; }

        [JsonProperty(Required = Required.Always)]
        public CompiledFeatureMetadata[] CompiledFeatureMetadata { get; set; }
    }
}