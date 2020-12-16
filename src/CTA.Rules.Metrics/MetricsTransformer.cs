using System.Collections.Generic;
using CTA.Rules.Models;

namespace CTA.Rules.Metrics
{
    /// <summary>
    /// Converts metrics data from raw form into serializable objects
    /// </summary>
    public class MetricsTransformer
    {
        internal static IEnumerable<GenericActionExecutionMetric> TransformGenericActionExecutions(MetricsContext context, ProjectResult projectResult)// Dictionary<string, List<GenericActionExecution>> executedActionsByProject)
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
    }
}