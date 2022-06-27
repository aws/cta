using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class FeatureDetectionMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "DetectedFeature";

        [JsonProperty("featureName", Order = 11)]
        public string FeatureName { get; set; }

        [JsonProperty("solutionPath", Order = 16)]
        public string SolutionPath { get; set; }

        [JsonProperty("projectGuid", Order = 17)]
        public string ProjectGuid { get; set; }

        public FeatureDetectionMetric(MetricsContext context, string featureName, string projectPath)
        {
            FeatureName = featureName;
            SolutionPath = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
            Language = GetLanguage(projectPath);
        }
    }
}
