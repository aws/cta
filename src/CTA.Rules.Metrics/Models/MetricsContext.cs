using System.Collections.Generic;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Config;

namespace CTA.Rules.Metrics
{
    public class MetricsContext
    {
        public string SolutionPath { get; set; }
        public string SolutionPathHash { get; set; }
        public Dictionary<string, string> ProjectGuidMap { get; set; }
        public MetricsContext(string solutionPath)
        {
            SolutionPath = solutionPath;
            SolutionPathHash = EncryptionHelper.ConvertToSHA256Hex(solutionPath);
            ProjectGuidMap = new Dictionary<string, string>();
        }
        public MetricsContext(string solutionPath, IEnumerable<AnalyzerResult> analyzerResults)
        {
            SolutionPath = solutionPath;
            SolutionPathHash = EncryptionHelper.ConvertToSHA256Hex(solutionPath);
            SetProjectGuidMap(analyzerResults);
        }

        public void AddProjectToMap(AnalyzerResult analyzerResult)
        {
            var projectName = analyzerResult.ProjectResult.ProjectFilePath;
            var projectGuid = analyzerResult.ProjectResult.ProjectGuid;
            ProjectGuidMap[projectName] = projectGuid;
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
