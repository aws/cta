using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class ReferencesMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "Namespace";

        [JsonProperty("reference", Order = 11)]
        public string Reference { get; set; }

        [JsonProperty("solutionPath", Order = 12)]
        public string SolutionPathHash { get; set; }

        public ReferencesMetric(MetricsContext context, string reference)
        {
            Reference = reference;
            SolutionPathHash = context.SolutionPathHash;
        }
    }

}
