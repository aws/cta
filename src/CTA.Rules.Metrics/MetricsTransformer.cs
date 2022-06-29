﻿using System.Collections.Generic;
using System.Linq;
using CTA.FeatureDetection.Common.Models;
using CTA.Rules.Config;
using CTA.Rules.Metrics.Models.WebForms;
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
            var sourceVersions = projectResult.SourceVersions;
            var targetVersionMetrics = new List<TargetVersionMetric>();

            for (int i = 0; i < targetVersions.Count; i++)
            {
                string sourceVersion = null;
                if (targetVersions.Count == sourceVersions.Count)
                {
                    sourceVersion = sourceVersions[i];
                }
                targetVersionMetrics.Add(new TargetVersionMetric(context, targetVersions[i], projectFile, sourceVersion));
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

        internal static IEnumerable<WebFormsActionMetric> TransformWebFormActionMetrics(MetricsContext context, ProjectResult projectResult)
        {
            var projectFile = projectResult.ProjectFile;
            var webFormActionMetrics = new List<WebFormsActionMetric>();
            foreach (var metric in projectResult.WebFormsMetricResults)
            {
                if(metric.ActionName == WebFormsActionType.FileConversion)
                    webFormActionMetrics.Add(new FileConversionMetric(context, metric.ChildAction, projectFile));
                else if(metric.ActionName == WebFormsActionType.ControlConversion)
                    webFormActionMetrics.Add(new ControlConversionMetric(context,metric.ChildAction,metric.NodeName, projectFile));
                else if(metric.ActionName == WebFormsActionType.ClassConversion)
                    webFormActionMetrics.Add(new ClassConversionMetric(context, metric.ChildAction, projectFile));
                else if(metric.ActionName == WebFormsActionType.DirectiveConversion)
                    webFormActionMetrics.Add(new DirectiveConversionMetric(context, metric.ChildAction, projectFile));
                else
                    LogHelper.LogInformation($"WebForms porting action not found with the name"+ metric.ActionName.ToString());

            }

            return webFormActionMetrics;
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
