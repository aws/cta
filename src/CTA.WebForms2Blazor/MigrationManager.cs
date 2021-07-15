﻿using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Models;
using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.Rules.Config;
using System;

namespace CTA.WebForms2Blazor
{
    public class MigrationManager
    {
        private const string MigrationTasksCompletedLogAction = "Migration Tasks Completed";

        private readonly string _inputProjectPath;
        private readonly string _outputProjectPath;

        private PortCoreConfiguration _projectConfiguration;
        private AnalyzerResult _analyzerResult;

        private ProjectAnalyzer _webFormsProjectAnalyzer;
        private ProjectBuilder _blazorProjectBuilder;

        private TaskManagerService _taskManager;
        private LifecycleManagerService _lifecycleManager;
        private ViewImportService _viewImportService;
        private WorkspaceManagerService _blazorWorkspaceManager;

        private ClassConverterFactory _classConverterFactory;
        private FileConverterFactory _fileConverterFactory;

        public MigrationManager(string inputProjectPath, string outputProjectPath, AnalyzerResult analyzerResult,
            PortCoreConfiguration projectConfiguration)
        {
            _inputProjectPath = inputProjectPath;
            _outputProjectPath = outputProjectPath;
            _analyzerResult = analyzerResult;
            _projectConfiguration = projectConfiguration;
        }

        public async Task PerformMigration()
        {
            LogHelper.LogInformation(string.Format(
                Constants.StartedFromToLogTemplate,
                GetType().Name,
                Constants.ProjectMigrationLogAction,
                _inputProjectPath,
                _outputProjectPath));

            // Order is important here
            InitializeProjectManagementStructures();
            InitializeServices();
            InitializeFactories();

            // Pass workspace build manager to factory constructor
            var fileConverterCollection = _fileConverterFactory.BuildMany(_webFormsProjectAnalyzer.GetProjectFileInfo());

            var migrationTasks = fileConverterCollection.Select(fileConverter =>
                // ContinueWith specifies the action to be run after each task completes,
                // in this case it sends each generated file to the project builder
                fileConverter.MigrateFileAsync().ContinueWith(generatedFiles =>
                {
                    try
                    {
                        // It's ok to use Task.Result here because the lambda within
                        // the ContinueWith block only executes once the original task
                        // is complete. Task.Result is also preferred because await
                        // would force our lambda expression to be async
                        foreach (FileInformation generatedFile in generatedFiles.Result)
                        {
                            _blazorProjectBuilder.WriteFileInformationToProject(generatedFile);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError(e, string.Format(Constants.OperationFailedLogTemplate, GetType().Name, Constants.FileMigrationLogAction));
                    }
                }
            ));

            // Combines migration tasks into a single task we can await
            await Task.WhenAll(migrationTasks);

            LogHelper.LogInformation(string.Format(Constants.GenericInformationLogTemplate, GetType().Name, MigrationTasksCompletedLogAction));

            // Adds _Imports.razor file to project
            _blazorProjectBuilder.WriteFileInformationToProject(_viewImportService.ConstructImportsFile());

            // TODO: Any necessary cleanup or last checks on new project

            LogHelper.LogInformation(string.Format(
                Constants.EndedFromToLogTemplate,
                GetType().Name,
                Constants.ProjectMigrationLogAction,
                _inputProjectPath,
                _outputProjectPath));
        }

        private void InitializeProjectManagementStructures()
        {
            _webFormsProjectAnalyzer = new ProjectAnalyzer(_inputProjectPath, _analyzerResult, _projectConfiguration);
            _blazorProjectBuilder = new ProjectBuilder(_outputProjectPath);
        }

        private void InitializeServices()
        {
            _taskManager = new TaskManagerService();
            _lifecycleManager = new LifecycleManagerService();
            _viewImportService = new ViewImportService();
            _blazorWorkspaceManager = new WorkspaceManagerService();
            _blazorWorkspaceManager.CreateSolutionFile();
        }

        private void InitializeFactories()
        {
            _classConverterFactory = new ClassConverterFactory(_inputProjectPath, _lifecycleManager, _taskManager);
            _fileConverterFactory = new FileConverterFactory(_inputProjectPath, _blazorWorkspaceManager, _webFormsProjectAnalyzer, _viewImportService, _classConverterFactory);
        }
    }
}