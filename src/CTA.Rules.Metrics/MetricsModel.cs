using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    /// <summary>
    /// Contains all serializable metrics objects
    /// </summary>

    public class MetricsContext
    {
        public string SolutionPath { get; set; }
        public string SolutionPathHash { get; set; }
        public Dictionary<string, string> ProjectGuidMap { get; set; }

        public MetricsContext(string solutionPath, IEnumerable<AnalyzerResult> analyzerResults)
        {
            SolutionPath = solutionPath;
            SolutionPathHash = EncryptionHelper.ConvertToSHA256Hex(solutionPath);
            SetProjectGuidMap(analyzerResults);
        }

        private void SetProjectGuidMap(IEnumerable<AnalyzerResult> analyzerResults)
        {
            ProjectGuidMap = new Dictionary<string, string>();
            foreach (var analyzerResult in analyzerResults)
            {
                var projectName = analyzerResult.ProjectResult.ProjectFilePath;
                var projectGuid = analyzerResult.ProjectResult.ProjectGuid;
                ProjectGuidMap[projectName] = projectGuid;
            }
        }
    }

    public abstract class CTAMetric
    {
        [JsonProperty("metricsType", Order = 1)]
        public string MetricsType => "CTA";
    }

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

    public class ReferencesMetric : CTAMetric
    {
        [JsonProperty("metricName", Order = 10)]
        public string MetricName => "Namespace";

        [JsonProperty("reference", Order = 11)]
        public string Reference { get; set; }

        [JsonProperty("solutionPath", Order = 12)]
        public string SolutionPathHash { get; set; }

        public ReferencesMetric(MetricsContext context, string reference)
        {
            Reference = reference;
            SolutionPathHash = context.SolutionPathHash;
        }
    }

    public class BuildErrorMetric : CTAMetric
    {
        // Match pattern for compiler error codes
        // example: "CS1234: "
        private const string BuildErrorCodePattern = "CS[0-9]{4}"; 

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
        }

        private string ExtractBuildErrorCode(string buildError)
        {
            var pattern = new Regex(BuildErrorCodePattern);
            var matches = pattern.Matches(buildError);
            var errorCode = matches.FirstOrDefault(m => buildError.StartsWith(m.Value))?.Value;

            return errorCode ?? string.Empty;
        }
    }
}
