using Codelyzer.Analysis;
using CTA.Rules.Config;
using System.Collections.Generic;

namespace CTA.Rules.Metrics
{
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
}
