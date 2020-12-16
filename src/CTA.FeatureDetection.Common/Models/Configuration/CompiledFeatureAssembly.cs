using Newtonsoft.Json;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class CompiledFeatureAssembly
    {
        [JsonProperty(Required = Required.Always)]
        public string AssemblyPath { get; set; }

        [JsonProperty(Required = Required.Always)]
        public CompiledFeatureNamespace[] CompiledFeatureNamespaces { get; set; }
    }
}