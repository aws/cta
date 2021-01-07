using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public abstract class CTAMetric
    {
        [JsonProperty("metricsType", Order = 1)]
        public string MetricsType => "CTA";
    }
}
