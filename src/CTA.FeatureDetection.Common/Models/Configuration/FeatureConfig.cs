using Newtonsoft.Json;

namespace CTA.FeatureDetection.Common.Models.Configuration
{
    /// <summary>
    /// Root object for feature configuration json files
    /// </summary>
    public class FeatureConfig
    {
        [JsonProperty(Required = Required.Always)]
        public FeatureGroup[] FeatureGroups { get; set; }
    }
}