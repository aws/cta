using Newtonsoft.Json;

namespace CTA.Rules.Models.Metrics
{
    public class DownloadedFilesMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "RulesFile";

        [JsonProperty("downloadedFile", Order = 11)]
        public string DownloadedFile { get; set; }

        [JsonProperty("solutionPath", Order = 12)]
        public string SolutionPathHash { get; set; }

        public DownloadedFilesMetric(MetricsContext context, string downloadedFile)
        {
            DownloadedFile = downloadedFile;
            SolutionPathHash = context.SolutionPathHash;
        }
    }
}
