using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class BuildErrorMetric : CTAMetric
    {
        // Match pattern for compiler error codes
        // example: "CS1234: "
        private const string BuildErrorCodePattern = "CS[0-9]{4}";
        private const string BuildErrorCodeDefaultPattern = "OTHER";

        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "BuildError";

        [JsonProperty("buildErrorCode", Order = 11)]
        public string BuildErrorCode { get; set; }

        [JsonProperty("buildError", Order = 12)]
        public string BuildError { get; set; }

        [JsonProperty("count", Order = 20)]
        public int Count { get; set; }

        [JsonProperty("solutionPath", Order = 30)]
        public string SolutionPathHash { get; set; }

        [JsonProperty("projectGuid", Order = 40)]
        public string ProjectGuid { get; set; }

        public BuildErrorMetric(MetricsContext context, string buildError, int count, string projectPath)
        {
            BuildErrorCode = ExtractBuildErrorCode(buildError);
            BuildError = buildError;
            Count = count;
            SolutionPathHash = context.SolutionPathHash;
            ProjectGuid = context.ProjectGuidMap.GetValueOrDefault(projectPath, "N/A");
            Language = GetLanguage(projectPath);
        }

        private string ExtractBuildErrorCode(string buildError)
        {
            var pattern = new Regex(BuildErrorCodePattern);
            var matches = pattern.Matches(buildError);
            var errorCode = matches.FirstOrDefault(m => buildError.StartsWith(m.Value))?.Value;

            return errorCode ?? BuildErrorCodeDefaultPattern;
        }
    }
}
