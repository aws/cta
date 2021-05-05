using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTA.Rules.Models.Metrics
{
    public class UpgradePackageMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "UpgradePackage";

        [JsonProperty("packageName", Order = 11)]
        public string PackageName { get; set; }

        [JsonProperty("packageVersion", Order = 12)]
        public string PackageVersion { get; set; }

        [JsonProperty("packageOriginalVersion", Order = 13)]
        public string PackageOriginalVersion { get; set; }

        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }

        public UpgradePackageMetric(MetricsContext context, PackageAction packageAction, string projectPath)
        {
            PackageName = packageAction.Name;
            PackageVersion = packageAction.Version;
            PackageOriginalVersion = packageAction.OriginalVersion;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
        }
    }
}
