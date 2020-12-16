using Newtonsoft.Json;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class CompiledFeatureMetadata
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ClassName { get; set; }
    }
}