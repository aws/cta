using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class CodeFileConverter : FileConverter
    {
        private readonly SemanticModel _fileModel;
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly ProjectAnalyzer _webFormsProjectAnaylzer;
        private readonly ClassConverterFactory _classConverterFactory;
        private readonly IEnumerable<ClassConverter> _classConverters = new List<ClassConverter>();

        public CodeFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ClassConverterFactory classConverterFactory,
            TaskManagerService taskManagerService) : base(sourceProjectPath, fullPath, taskManagerService)
        {
            // May not need this anymore but not sure yet
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            _webFormsProjectAnaylzer = webFormsProjectAnalyzer;
            _classConverterFactory = classConverterFactory;

            try
            {
                _fileModel = _webFormsProjectAnaylzer.AnalyzerResult.ProjectBuildResult?.SourceFileBuildResults?
                    .Single(r => r.SourceFileFullPath.EndsWith(RelativePath))?.SemanticModel;
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"Exception occurred when trying to retrieve semantic model for the file {RelativePath}. " +
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
            // NOTE: We don't provide an reference to the caught exception because it would just be a
            // standard aggregate exception of the exceptions that arose in the migration tasks, instead
            // we choose to log an exception for each of the failed tasks to provide more info
            catch
            {
                var failedMigrationTasks = classMigrationTasks.Where(t => t.Item2.Status != TaskStatus.RanToCompletion);

                foreach (var failedTask in failedMigrationTasks)
                {
                    LogHelper.LogError(failedTask.Item2.Exception, $"Failed to migrate {failedTask.Item1.OriginalClassName} class " +
                        $"located at {failedTask.Item1.FullPath}");
                }
            }

            var result = classMigrationTasks.Where(t => t.Item2.Status == TaskStatus.RanToCompletion).SelectMany(t => t.Item2.Result);

            LogEnd();

            return result;
        }
    }
}
