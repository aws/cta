using CTA.Rules.Metrics;
using Newtonsoft.Json;

namespace CTA.WebForms2Blazor.Metrics
{
    public class WebFormMetric :CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "WebFormsAction";

        [JsonProperty("childActionName", Order = 11)]
        public string ChildActionName { get; set; }
        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }
    }
}
