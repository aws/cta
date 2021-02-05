using System.Collections.Generic;
using CTA.Rules.Models;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class ActionPackageMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "ActionPackage";

        [JsonProperty("packageName", Order = 11)]
        public string PackageName { get; set; }

        [JsonProperty("packageVersion", Order = 11)]
        public string PackageVersion { get; set; }

        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }

        public ActionPackageMetric(MetricsContext context, PackageAction packageAction, string projectPath)
        {
            PackageName = packageAction.Name;
            PackageVersion = packageAction.Version;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
