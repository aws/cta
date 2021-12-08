using System.Collections.Generic;
using CTA.Rules.Metrics;
using Newtonsoft.Json;

namespace CTA.WebForms2Blazor.Metrics
{
    public class DirectiveConversionMetric : WebFormMetric
    {
        [JsonProperty("actionName", Order = 11)]
        public string ActionName => "DirectiveConversion";

        public DirectiveConversionMetric(MetricsContext context, string childActionName, string projectPath)
        {
            ChildActionName = childActionName;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
