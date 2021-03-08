using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.FeatureDetection.Common.Models;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class PortSolutionResultReportGenerator
    {
        public MetricsContext Context { get; set; }
        public PortSolutionResult PortSolutionResult { get; set; }
        public Dictionary<string, FeatureDetectionResult> FeatureDetectionResults { get; set; }
        public IEnumerable<ReferencesMetric> ReferencesMetrics { get; set; }
        public IEnumerable<DownloadedFilesMetric> DownloadedFilesMetrics { get; set; }
        public IEnumerable<TargetVersionMetric> TargetVersionMetrics { get; set; }
        public IEnumerable<UpgradePackageMetric> UpgradePackageMetrics { get; set; }
        public IEnumerable<ActionPackageMetric> ActionPackageMetrics { get; set; }
        public IEnumerable<GenericActionMetric> GenericActionMetrics { get; set; }
        public IEnumerable<FeatureDetectionMetric> FeatureDetectionMetrics { get; set; }
        public IEnumerable<GenericActionExecutionMetric> GenericActionExecutionMetrics { get; set; }
        public IEnumerable<BuildErrorMetric> BuildErrorMetrics { get; set; }
        public string AnalyzeSolutionResultJsonReport { get; set; }
        public string PortSolutionResultJsonReport { get; set; }
        public string PortSolutionResultTextReport { get; set; }

        public PortSolutionResultReportGenerator(MetricsContext context, PortSolutionResult portSolutionResult, Dictionary<string, FeatureDetectionResult> featureDetectionResults)
        {
            Context = context;
            PortSolutionResult = portSolutionResult;
            FeatureDetectionResults = featureDetectionResults;
        }

        public PortSolutionResultReportGenerator(MetricsContext context,PortSolutionResult portSolutionResult)
        {
            Context = context;
            PortSolutionResult = portSolutionResult;
            FeatureDetectionResults = new Dictionary<string, FeatureDetectionResult>();
        }

        public void GenerateAndExportReports()
        {
            GenerateMetrics();
            GeneratePortSolutionResultJsonReport();
            GeneratePortSolutionResultTextReport();

            ExportStringToFile(PortSolutionResult.SolutionPath, "PortSolutionResult.json", PortSolutionResultJsonReport);
            ExportStringToFile(PortSolutionResult.SolutionPath, "PortSolutionResult.txt", PortSolutionResultTextReport);
        }

        public void GenerateAnalysisReport()
        {
            GenerateAnalysisMetrics();
            GenerateAnalyzeSolutionResultJsonReport();
        }

        private void GenerateAnalysisMetrics()
        {
            var upgradePackageMetrics = new List<UpgradePackageMetric>();
            var actionPackageMetrics = new List<ActionPackageMetric>();
            var actionMetrics = new List<GenericActionMetric>();
            var featureDetectionMetrics = new List<FeatureDetectionMetric>();

            foreach (var projectResult in PortSolutionResult.ProjectResults)
            {
                upgradePackageMetrics.AddRange(MetricsTransformer.TransformUpgradePackages(Context, projectResult));
                actionPackageMetrics.AddRange(MetricsTransformer.TransformActionPackages(Context, projectResult));
                actionMetrics.AddRange(MetricsTransformer.TransformProjectActions(Context, projectResult));
            }
            featureDetectionMetrics.AddRange(MetricsTransformer.TransformFeatureDetectionResults(Context, FeatureDetectionResults));

            UpgradePackageMetrics = upgradePackageMetrics;
            ActionPackageMetrics = actionPackageMetrics;
            GenericActionMetrics = actionMetrics;
            FeatureDetectionMetrics = featureDetectionMetrics;
        }

        private void GenerateMetrics()
        {
            // Gather solution-level metrics
            BuildErrorMetrics = MetricsTransformer.TransformBuildErrors(Context, PortSolutionResult.BuildErrors);
            ReferencesMetrics = MetricsTransformer.TransformReferences(Context, PortSolutionResult.References);
            DownloadedFilesMetrics = MetricsTransformer.TransformDownloadedFiles(Context, PortSolutionResult.DownloadedFiles);

            // Gather project-level metrics
            var targetVersionMetrics = new List<TargetVersionMetric>();
            var upgradePackageMetrics = new List<UpgradePackageMetric>();
            var actionPackageMetrics = new List<ActionPackageMetric>();
            var actionExecutionMetrics = new List<GenericActionExecutionMetric>();

            foreach (var projectResult in PortSolutionResult.ProjectResults)
            {
                targetVersionMetrics.AddRange(MetricsTransformer.TransformTargetVersions(Context, projectResult));
                upgradePackageMetrics.AddRange(MetricsTransformer.TransformUpgradePackages(Context, projectResult));
                actionPackageMetrics.AddRange(MetricsTransformer.TransformActionPackages(Context, projectResult));
                actionExecutionMetrics.AddRange(MetricsTransformer.TransformGenericActionExecutions(Context, projectResult));
            }

            TargetVersionMetrics = targetVersionMetrics;
            UpgradePackageMetrics = upgradePackageMetrics;
            ActionPackageMetrics = actionPackageMetrics;
            GenericActionExecutionMetrics = actionExecutionMetrics;
        }

        private string GenerateAnalyzeSolutionResultJsonReport()
        {
            var allMetrics = new List<CTAMetric>();
            allMetrics = allMetrics
                .Concat(UpgradePackageMetrics)
                .Concat(ActionPackageMetrics)
                .Concat(GenericActionMetrics)
                .Concat(FeatureDetectionMetrics)
                .ToList();

            AnalyzeSolutionResultJsonReport = JsonConvert.SerializeObject(allMetrics);
            return AnalyzeSolutionResultJsonReport;
        }

        private string GeneratePortSolutionResultJsonReport()
        {
            var allMetrics = new List<CTAMetric>();
            allMetrics = allMetrics.Concat(ReferencesMetrics)
                .Concat(DownloadedFilesMetrics)
                .Concat(TargetVersionMetrics)
                .Concat(UpgradePackageMetrics)
                .Concat(ActionPackageMetrics)
                .Concat(GenericActionExecutionMetrics)
                .Concat(BuildErrorMetrics)
                .ToList();

            PortSolutionResultJsonReport = JsonConvert.SerializeObject(allMetrics);
            return PortSolutionResultJsonReport;
        }

        private string GeneratePortSolutionResultTextReport()
        {
            PortSolutionResultTextReport = PortSolutionResult.ToString();
            return PortSolutionResultTextReport;
        }

        private static void ExportStringToFile(string projectOrSolutionPath, string fileName, string content)
        {
            try
            {
                var filePath = GetFilePath(projectOrSolutionPath, fileName);
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.Message);
            }
        }

        private static string GetFilePath(string projectPath, string fileName) => Path.Combine(Directory.GetParent(projectPath).FullName, fileName);
    }
}
