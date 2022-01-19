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

            // Need to have ToList call here to enumerate the collection and ensure class
            // converters are running before we retire this file converter task
            var classMigrationTasks = _classConverters.Select(converter => converter.MigrateClassAsync()).ToList();

            // We want to do our cleanup now because from this point on all migration tasks
            // are done by class converters and we want to make sure that we retire the task
            // related to this file converter before we await
            DoCleanUp();

            var allMigrationTasks = Task.WhenAll(classMigrationTasks);

            try
            {
                await allMigrationTasks;
            }
            // We don't provide a reference for the thrown exception here because await auto-
            // unwraps aggregate exceptions and throws only the first encountered exception
            catch
            {
                // We access allMigrationTasks.Exception instead to provide the original AggregateException
                var allExceptions = allMigrationTasks.Exception.Flatten().InnerExceptions;

                foreach (Exception e in allExceptions)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to migrate class");
                }
            }

            var result = classMigrationTasks.Where(t => t.Status == TaskStatus.RanToCompletion).SelectMany(t => t.Result);

            LogEnd();

            return result;
        }
    }
}
