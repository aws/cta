using System.Collections.Generic;
using CTA.Rules.Metrics;
using Newtonsoft.Json;

namespace CTA.WebForms2Blazor.Metrics
{
    public class ControlConversionMetric : WebFormMetric
    {
        [JsonProperty("actionName", Order = 11)]
        public string ActionName => "ControlConversion";
        [JsonProperty("nodeName", Order = 12)]
        public string NodeName { get; set; }

        public ControlConversionMetric(MetricsContext context, string childActionName, string projectPath, string nodeName)
        {
            ChildActionName = childActionName;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
            NodeName = nodeName;
        }
    }
}
