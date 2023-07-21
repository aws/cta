﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms.Factories;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Metrics;
using CTA.WebForms.ProjectManagement;
using CTA.WebForms.Services;

namespace CTA.WebForms
{
    public class MigrationManager
    {
        private const string MigrationTasksCompletedLogAction = "Migration Tasks Completed";

        private readonly string _inputProjectPath;
        private readonly ProjectResult _projectResult;

        private readonly ProjectConfiguration _projectConfiguration;
        private readonly AnalyzerResult _analyzerResult;

        private ProjectAnalyzer _webFormsProjectAnalyzer;
        private ProjectBuilder _blazorProjectBuilder;

        private TaskManagerService _taskManager;
        private LifecycleManagerService _lifecycleManager;
        private ViewImportService _viewImportService;
        private CodeBehindReferenceLinkerService _codeBehindLinkerService;
        private ProgramCsService _programCsService;
        private AppRazorService _appRazorService;
        private HostPageService _hostPageService;
        private WorkspaceManagerService _blazorWorkspaceManager;

        private ClassConverterFactory _classConverterFactory;
        private FileConverterFactory _fileConverterFactory;
        private TagConfigParser _tagConfigParser;
        private WebFormMetricContext _metricsContext;

        public MigrationManager(string inputProjectPath, AnalyzerResult analyzerResult,
            ProjectConfiguration projectConfiguration, ProjectResult projectResult)
        {
            _inputProjectPath = inputProjectPath;
            _analyzerResult = analyzerResult;
            _projectConfiguration = projectConfiguration;
            _projectResult = projectResult;
            _tagConfigParser = new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath);
            _metricsContext = new WebFormMetricContext();
        }

        public async Task<WebFormsPortingResult> PerformMigration()
        {
            LogHelper.LogInformation(string.Format(
                Constants.StartedOfLogTemplate,
                GetType().Name,
                Constants.ProjectMigrationLogAction,
                _inputProjectPath));

            // Order is important here
            InitializeProjectManagementStructures();
            InitializeServices();
            InitializeFactories();

            // Pass workspace build manager to factory constructor
            var fileConverterCollection = _fileConverterFactory.BuildMany(_webFormsProjectAnalyzer.GetProjectFileInfo());

            var ignorableFileInfo = _webFormsProjectAnalyzer.GetProjectIgnoredFiles();
            foreach (var fileInfo in ignorableFileInfo)
            {
                _blazorProjectBuilder.DeleteFileAndEmptyDirectories(fileInfo.FullName, _inputProjectPath);
            }

            var deleteFilesStack = new ConcurrentBag<(string filePath, string pathLimit)>();
            var writeFilesStack = new ConcurrentBag<FileInformation>();

            _taskManager.InitializeWatchdog();
            var migrationTasks = fileConverterCollection.Select(fileConverter =>
                // ContinueWith specifies the action to be run after each task completes,
                // in this case it sends each generated file to the project builder
                fileConverter.MigrateFileAsync().ContinueWith(generatedFiles =>
                {
                    try
                    {
                        deleteFilesStack.Add((fileConverter.FullPath, _inputProjectPath));

                        // It's ok to use Task.Result here because the lambda within
                        // the ContinueWith block only executes once the original task
                        // is complete. Task.Result is also preferred because await
                        // would force our lambda expression to be async
                        foreach (FileInformation generatedFile in generatedFiles.Result)
                        {
                            if (generatedFile != null)
                            {
                                writeFilesStack.Add(generatedFile);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError(
                            e, Rules.Config.Constants.WebFormsErrorTag +
                        string.Format(Constants.OperationFailedLogTemplate, GetType().Name, Constants.FileMigrationLogAction));
                    }
                }
            ));

            // Combines migration tasks into a single task we can await
            await Task.WhenAll(migrationTasks).ConfigureAwait(false);
            _taskManager.DisableWatchdog();

            DeleteUsedSourceFiles(deleteFilesStack);
            WriteGeneratedFiles(writeFilesStack);
            WriteServiceDerivedFiles();

            LogHelper.LogInformation(string.Format(Constants.GenericInformationLogTemplate, GetType().Name, MigrationTasksCompletedLogAction));

            // TODO: Get new nuget packages from view import service and add to project references

            var result = new WebFormsPortingResult() { Metrics = _metricsContext.Transform() };

            // TODO: Any necessary cleanup or last checks on new project

            LogHelper.LogInformation(string.Format(
                Constants.EndedOfLogTemplate,
                GetType().Name,
                Constants.ProjectMigrationLogAction,
                _inputProjectPath));

            return result;
        }

        private void DeleteUsedSourceFiles(IEnumerable<(string filePath, string pathLimit)> sourceFiles)
        {
            foreach (var sourceFile in sourceFiles ?? Enumerable.Empty<(string filePath, string pathLimit)>())
            {
                try
                {
                    _blazorProjectBuilder.DeleteFileAndEmptyDirectories(sourceFile.filePath, sourceFile.pathLimit);
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to delete file " +
                        $"at {sourceFile.filePath}");
                }
            }
        }

        private void WriteGeneratedFiles(IEnumerable<FileInformation> generatedFiles)
        {
            foreach (var generatedFile in generatedFiles ?? Enumerable.Empty<FileInformation>())
            {
                try
                {
                    _blazorProjectBuilder.WriteFileInformationToProject(generatedFile);
                }
                catch (Exception e)
                {
                    if (generatedFile == null)
                    {
                        LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to write WebForms porting " +
                            $"result, file information was null");
                    }
                    else
                    {
                        LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to write WebForms porting " +
                            $"result to {generatedFile.RelativePath}");
                    }
                }
            }
        }

        private void WriteServiceDerivedFiles()
        {
            _blazorProjectBuilder.WriteFileInformationToProject(_viewImportService.ConstructImportsFile());
            _blazorProjectBuilder.WriteFileInformationToProject(_programCsService.ConstructProgramCsFile());
            _blazorProjectBuilder.WriteFileInformationToProject(_appRazorService.ConstructAppRazorFile());
            _blazorProjectBuilder.WriteFileInformationToProject(_hostPageService.ConstructHostPageFile());
        }

        private void InitializeProjectManagementStructures()
        {
            _webFormsProjectAnalyzer = new ProjectAnalyzer(_inputProjectPath, _analyzerResult, _projectConfiguration, _projectResult);
            _blazorProjectBuilder = new ProjectBuilder(_inputProjectPath);
        }

        private void InitializeServices()
        {
            _taskManager = new TaskManagerService(TaskManagerService.TaskStallTimeoutMs.Medium);
            _lifecycleManager = new LifecycleManagerService();
            _viewImportService = new ViewImportService();
            _codeBehindLinkerService = new CodeBehindReferenceLinkerService();
            _programCsService = new ProgramCsService();
            _appRazorService = new AppRazorService();
            _hostPageService = new HostPageService();
            _blazorWorkspaceManager = new WorkspaceManagerService();

            // By convention, we expect the root namespace to share a name with the project
            // root folder, we normalize the folder name before using it in case the folder name
            // is not a valid identifier
            var rootNamespace = Utilities.NormalizeNamespaceIdentifier(_analyzerResult.ProjectResult.ProjectName);
            _programCsService.ProgramCsNamespace = rootNamespace;
            _hostPageService.HostNamespace = rootNamespace;
            _blazorWorkspaceManager.CreateSolutionFile();
        }

        private void InitializeFactories()
        {
            _classConverterFactory = new ClassConverterFactory(
                _inputProjectPath,
                _lifecycleManager,
                _taskManager,
                _codeBehindLinkerService,
                _metricsContext);
            _fileConverterFactory = new FileConverterFactory(
                _inputProjectPath,
                _blazorWorkspaceManager,
                _webFormsProjectAnalyzer,
                _viewImportService,
                _codeBehindLinkerService,
                _classConverterFactory,
                _hostPageService,
                _taskManager,
                _tagConfigParser,
                _metricsContext);
        }
    }
}
