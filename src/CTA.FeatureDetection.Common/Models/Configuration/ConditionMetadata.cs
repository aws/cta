using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    public class ConditionMetadata
    {
        [JsonProperty(Required = Required.Default)]
        public bool MatchType { get; set; } = true;

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionType Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}