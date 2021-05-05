using Newtonsoft.Json;

namespace CTA.Rules.Models.Metrics
{
    public abstract class CTAMetric
    {
        [JsonProperty("metricsType", Order = 1)]
        public string MetricsType => "CTA";
    }
}
