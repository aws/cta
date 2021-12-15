using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics.Models.WebForms
{
    public class ControlConversionMetric : WebFormsActionMetric
    {
        [JsonProperty("actionName", Order = 11)]
        public string ActionName => "ControlConversion";

        public ControlConversionMetric(MetricsContext context, string childActionName, string nodeName, string projectPath)
            : base(context, childActionName, projectPath)
        {
            NodeName = nodeName;
        }
    }
}
