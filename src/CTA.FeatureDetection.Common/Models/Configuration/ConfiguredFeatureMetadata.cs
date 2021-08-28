using CTA.FeatureDetection.Common.Reporting;
using Newtonsoft.Json;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class ConfiguredFeatureMetadata
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Default)]
        public FeatureCategory FeatureCategory { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty(Required = Required.Default)]
        public bool IsLinuxCompatible { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConditionMetadata Condition { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConditionGroupMetadata ConditionGroup { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConditionGroupMetadata[] ConditionGroups { get; set; }
    }
}
