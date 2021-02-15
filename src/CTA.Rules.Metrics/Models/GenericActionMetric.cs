using System.Collections.Generic;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class GenericActionMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "GenericAction";

        [JsonProperty("actionName", Order = 11)]
        public string ActionName { get; set; }

        [JsonProperty("actionType", Order = 12)]
        public string ActionType { get; set; }

        [JsonProperty("actionValue", Order = 13)]
        public string ActionValue { get; set; }

        [JsonProperty("solutionPath", Order = 16)]
        public string SolutionPath { get; set; }

        [JsonProperty("projectGuid", Order = 17)]
        public string ProjectGuid { get; set; }

        [JsonProperty("filePath", Order = 18)]
        public string FilePath { get; set; }

        public GenericActionMetric(MetricsContext context, GenericAction action, string filePath, string projectPath)
        {
            ActionName = action.Name;
            ActionType = action.Type;
            ActionValue = action.Value;
            SolutionPath = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");

            if (ActionType == Constants.Project)
            {
                FilePath = ProjectGuid;
            }
            else
            {
                FilePath = string.IsNullOrEmpty(filePath) ? "N/A" : EncryptionHelper.ConvertToSHA256Hex(filePath);
            }
        }
    }
}
