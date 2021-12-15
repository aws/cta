using System;
using System.Collections.Generic;
using System.Text;
using CTA.Rules.Models;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics.Models.WebForms
{
    public class WebFormsActionMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "WebFormActions";

        [JsonProperty("childActionName", Order = 11)]
        public string ChildActionName { get; set; }

        [JsonProperty("nodeName", Order = 12, NullValueHandling = NullValueHandling.Ignore)]
        public string NodeName { get; set; }

        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }

        public WebFormsActionMetric(MetricsContext context, string childActionName, string projectPath)
        {
            ChildActionName = childActionName;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
