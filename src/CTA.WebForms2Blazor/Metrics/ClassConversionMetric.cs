using System;
using System.Collections.Generic;
using System.Text;
using CTA.Rules.Metrics;
using Newtonsoft.Json;

namespace CTA.WebForms2Blazor.Metrics
{
    public class ClassConversionMetric : WebFormMetric
    {
        [JsonProperty("actionName", Order = 11)]
        public string ActionName => "ClassConversion";

        public ClassConversionMetric(MetricsContext context, string childActionName, string projectPath)
        {
            ChildActionName = childActionName;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
