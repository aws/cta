using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms.ClassConverters;
using CTA.WebForms.Factories;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Metrics;
using CTA.WebForms.ProjectManagement;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms.FileConverters
{
    public class CodeFileConverter : FileConverter
    {
        private const string ChildActionType = "CodeFileConverter";
        private readonly SemanticModel _fileModel;
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly ProjectAnalyzer _webFormsProjectAnaylzer;
        private readonly ClassConverterFactory _classConverterFactory;
        private readonly IEnumerable<ClassConverter> _classConverters;
        private WebFormMetricContext _metricsContext;

        public CodeFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ClassConverterFactory classConverterFactory,
            TaskManagerService taskManagerService,
            WebFormMetricContext metricsContext) : base(sourceProjectPath, fullPath, taskManagerService)
        {
            // May not need this anymore but not sure yet
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            _webFormsProjectAnaylzer = webFormsProjectAnalyzer;
            _classConverterFactory = classConverterFactory;
            _metricsContext = metricsContext;

            try
            {
                _fileModel = _webFormsProjectAnaylzer.AnalyzerResult.ProjectBuildResult?.SourceFileBuildResults?
                    .Single(r => r.SourceFileFullPath.EndsWith(RelativePath))?.SemanticModel;
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Exception occurred when trying to retrieve semantic model for the file {RelativePath}. " +
                                      "Semantic Model will default to null.");
            }

            if (_fileModel != null)
            {
                _classConverters = classConverterFactory.BuildMany(RelativePath, _fileModel);
            }
        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectActionMetrics(WebFormsActionType.FileConversion, ChildActionType);

            // Store migration tasks alongside converter so we can log context where necessary
            var classMigrationTasks = new List<(ClassConverter, Task<IEnumerable<FileInformation>>)>();

            foreach (var converter in _classConverters)
            {
                classMigrationTasks.Add((converter, converter.MigrateClassAsync()));
            }

            // We want to do our cleanup now because from this point on all migration tasks
            // are done by class converters and we want to make sure that we retire the task
            // related to this file converter before we await
            DoCleanUp();

            var allMigrationTasks = Task.WhenAll(classMigrationTasks.Select(t => t.Item2));

            try
            {
                await allMigrationTasks;
            }
            // NOTE: We don't provide a reference to the caught exception here because any
            // encountered exceptions will be logged when examining the individual failed tasks
            catch
            {
                // NOTE: While using await does automatically unwrap AggregateExceptions when they
                // only contain 1 exception, component tasks of Task.WhenAll() will never have their
                // AggregateExceptions unwrapped when the WhenAll task is awaited

                var failedMigrationTasks = classMigrationTasks
                    // Select failed tasks only
                    .Where(t => t.Item2.Status != TaskStatus.RanToCompletion)
                    // We can be fairly certain that failed tasks will only have a single exception,
                    // but we use InnerExceptions instead of InnerException just to be thorough
                    .SelectMany(t =>
                        t.Item2.Exception?.InnerExceptions?.Select(e => (t.Item1, e))
                        ?? Enumerable.Empty<(ClassConverter, Exception)>()
                    );

                foreach (var failedTask in failedMigrationTasks)
                {
                    LogHelper.LogError(failedTask.Item2, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to migrate {failedTask.Item1.OriginalClassName} class " +
                        $"located at {failedTask.Item1.FullPath}");
                }
            }

            var result = classMigrationTasks.Where(t => t.Item2.Status == TaskStatus.RanToCompletion).SelectMany(t => t.Item2.Result);

            LogEnd();

            return result;
        }
    }
}
