using CTA.FeatureDetection.Common.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class ConditionGroupMetadata
    {
        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public JoinOperator JoinOperator { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ConditionMetadata[] Conditions { get; set; }
    }
}