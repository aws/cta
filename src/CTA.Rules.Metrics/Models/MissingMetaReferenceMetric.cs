using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class MissingMetaReferenceMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "MissingMetaReference";

        [JsonProperty("metaReference", Order = 11)]
        public string MetaReference { get; set; }

        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }

        public MissingMetaReferenceMetric(MetricsContext context, string metaReference, string projectPath)
        {
            MetaReference = metaReference;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
