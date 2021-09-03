using System.Collections.Generic;
using System.Linq;
using CTA.FeatureDetection.Common.Models;
using CTA.Rules.Models;

namespace CTA.Rules.Metrics
{
    /// <summary>
    /// Converts metrics data from raw form into serializable objects
    /// </summary>
    public class MetricsTransformer
    {
        internal static IEnumerable<GenericActionExecutionMetric> TransformGenericActionExecutions(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var executedActionsByFile = projectResult.ExecutedActions;
            var genericActionExecutionsMetrics = new List<GenericActionExecutionMetric>();
            foreach (var kvp in executedActionsByFile)
            {
                var fileName = kvp.Key;
                var actionExecutions = kvp.Value;
                foreach (var actionExecution in actionExecutions)
                {
                    genericActionExecutionsMetrics.Add(new GenericActionExecutionMetric(context, actionExecution, projectFile));
                }
            }

            return genericActionExecutionsMetrics;
        }

        internal static IEnumerable<GenericActionMetric> TransformProjectActions(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var detectedActionsByFile = projectResult.ProjectActions?.FileActions.ToList();
            var genericActions = new List<GenericActionMetric>();
            foreach (var fileAction in detectedActionsByFile)
            {
                var fileName = fileAction.FilePath;
                var actionExecutions = fileAction.AllActions;
                foreach (var actionExecution in actionExecutions)
                {
                    genericActions.Add(new GenericActionMetric(context, actionExecution, fileName, projectFile));
                }
            }

            return genericActions;
        }

        internal static IEnumerable<TargetVersionMetric> TransformTargetVersions(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var targetVersions = projectResult.TargetVersions;
            var targetVersionMetrics = new List<TargetVersionMetric>();
            foreach (var targetVersion in targetVersions)
            {
                targetVersionMetrics.Add(new TargetVersionMetric(context, targetVersion, projectFile));
            }

            return targetVersionMetrics;
        }

        internal static IEnumerable<UpgradePackageMetric> TransformUpgradePackages(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var upgradePackages = projectResult.UpgradePackages;
            var upgradePackagesMetric = new List<UpgradePackageMetric>();
            foreach (var upgradePackage in upgradePackages)
            {
                upgradePackagesMetric.Add(new UpgradePackageMetric(context, upgradePackage, projectFile));
            }

            return upgradePackagesMetric;
        }


        internal static IEnumerable<ActionPackageMetric> TransformActionPackages(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var actionPackages = projectResult.ActionPackages;
            var actionPackagesMetric = new List<ActionPackageMetric>();
            foreach (var actionPackage in actionPackages)
            {
                actionPackagesMetric.Add(new ActionPackageMetric(context, actionPackage, projectFile));
            }

            return actionPackagesMetric;
        }

        internal static IEnumerable<DownloadedFilesMetric> TransformDownloadedFiles(MetricsContext context, IEnumerable<string> downloadedFiles)
        {
            var downloadedFilesMetrics = new List<DownloadedFilesMetric>();
            foreach (var downloadedFile in downloadedFiles)
            {
                downloadedFilesMetrics.Add(new DownloadedFilesMetric(context, downloadedFile));
            }

            return downloadedFilesMetrics;
        }

        internal static IEnumerable<ReferencesMetric> TransformReferences(MetricsContext context, IEnumerable<string> references)
        {
            var referencesMetrics = new List<ReferencesMetric>();
            foreach (var reference in references)
            {
                referencesMetrics.Add(new ReferencesMetric(context, reference));
            }

            return referencesMetrics;
        }

        internal static IEnumerable<MissingMetaReferenceMetric> TransformMissingMetaReferences(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var missingMetaReferences = projectResult.MissingMetaReferences;
            var missingMetaReferenceMetrics = new List<MissingMetaReferenceMetric>();
            foreach (var reference in missingMetaReferences)
            {
                missingMetaReferenceMetrics.Add(new MissingMetaReferenceMetric(context, reference, projectFile));
            }

            return missingMetaReferenceMetrics;
        }

        internal static IEnumerable<BuildErrorMetric> TransformBuildErrors(MetricsContext context,
            Dictionary<string, Dictionary<string, int>> buildErrorsByProject)
        {
            var buildErrorMetrics = new List<BuildErrorMetric>();
            foreach (var project in buildErrorsByProject.Keys)
            {
                var buildErrorCounts = buildErrorsByProject[project];
                foreach (var buildError in buildErrorCounts.Keys)
                {
                    var count = buildErrorCounts[buildError];
                    buildErrorMetrics.Add(new BuildErrorMetric(context, buildError, count, project));
                }
            }

            return buildErrorMetrics;
        }

        internal static IEnumerable<FeatureDetectionMetric> TransformFeatureDetectionResults(MetricsContext context,
            Dictionary<string, FeatureDetectionResult> featureDetectionResults)
        {
            var featureDetectionMetrics = new List<FeatureDetectionMetric>();
            foreach (var kvp in featureDetectionResults)
            {
                var featureDetectionResult = kvp.Value;
                var metrics = featureDetectionResult.PresentFeatures.Select(featureName =>
                    new FeatureDetectionMetric(context, featureName, featureDetectionResult.ProjectPath));

                featureDetectionMetrics.AddRange(metrics);
            }

            return featureDetectionMetrics;
        }
    }
}
