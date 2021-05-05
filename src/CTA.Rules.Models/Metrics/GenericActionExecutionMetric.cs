using System.Collections.Generic;
using CTA.Rules.Config;
using Newtonsoft.Json;

namespace CTA.Rules.Models.Metrics
{
    public class GenericActionExecutionMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "ActionExecution";

        [JsonProperty("actionName", Order = 11)]
        public string ActionName { get; set; }

        [JsonProperty("actionType", Order = 12)]
        public string ActionType { get; set; }

        [JsonProperty("actionValue", Order = 13)]
        public string ActionValue { get; set; }

        [JsonProperty("timesRun", Order = 14)]
        public int TimesRun { get; set; }

        [JsonProperty("invalidExecutions", Order = 15)]
        public int InvalidExecutions { get; set; }

        [JsonProperty("solutionPath", Order = 16)]
        public string SolutionPath { get; set; }

        [JsonProperty("projectGuid", Order = 17)]
        public string ProjectGuid { get; set; }

        [JsonProperty("filePath", Order = 18)]
        public string FilePath { get; set; }

        public GenericActionExecutionMetric(MetricsContext context, GenericActionExecution action, string projectPath)
        {
            ActionName = action.Name;
            ActionType = action.Type;
            ActionValue = action.Value;
            TimesRun = action.TimesRun;
            InvalidExecutions = action.InvalidExecutions;
            SolutionPath = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");

            if (ActionType == Constants.Project)
            {
                FilePath = ProjectGuid;
            }
            else
            {
                FilePath = string.IsNullOrEmpty(action.FilePath) ? "N/A" : EncryptionHelper.ConvertToSHA256Hex(action.FilePath);
            }
        }
    }
}
