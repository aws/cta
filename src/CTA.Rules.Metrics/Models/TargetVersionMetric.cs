using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class TargetVersionMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "TargetVersion";

        [JsonProperty("targetVersion", Order = 11)]
        public string TargetVersion { get; set; }

        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }

        public TargetVersionMetric(MetricsContext context, string targetVersion, string projectPath)
        {
            TargetVersion = targetVersion;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
