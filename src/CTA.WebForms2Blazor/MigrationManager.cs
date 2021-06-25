using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor
{
    public class MigrationManager
    {
        private readonly string _inputProjectPath;
        private readonly string _outputProjectPath;

        // At the moment these are only being used in the
        // PerformMigration method, but they are likely to
        // be used in other places later on. Leaving them
        // in for now
        private ProjectAnalyzer _webFormsProjectAnalyzer;
        private ProjectBuilder _blazorProjectBuilder;

        private WorkspaceManagerService _webFormsWorkspaceManager;
        private WorkspaceManagerService _blazorWorkspaceManager;
        private ClassConverterFactory _classConverterFactory;
        private FileConverterFactory _fileConverterFactory;

        public MigrationManager(string inputProjectPath, string outputProjectPath)
        {
            _inputProjectPath = inputProjectPath;
            _outputProjectPath = outputProjectPath;
        }

        public async Task PerformMigration()
        {
            SetUpWorkspaceManagers();

            _webFormsProjectAnalyzer = new ProjectAnalyzer(_inputProjectPath);
            _blazorProjectBuilder = new ProjectBuilder(_outputProjectPath);
            _classConverterFactory = new ClassConverterFactory(_inputProjectPath);
            _fileConverterFactory = new FileConverterFactory(_inputProjectPath, _blazorWorkspaceManager, _webFormsWorkspaceManager, _classConverterFactory);

            // Pass workspace build manager to factory constructor
            var fileConverterCollection = _fileConverterFactory.BuildMany(_webFormsProjectAnalyzer.GetProjectFileInfo());

            var migrationTasks = fileConverterCollection.Select(fileConverter =>
                // ContinueWith specifies the action to be run after each task completes,
                // in this case it sends each generated file to the project builder
                fileConverter.MigrateFileAsync().ContinueWith(generatedFiles =>
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
            ));

            // Combines migration tasks into a single task we can await
            await Task.WhenAll(migrationTasks);

            // TODO: Any necessary cleanup or last checks on new project
        }

        private void SetUpWorkspaceManagers()
        {
            _webFormsWorkspaceManager = new WorkspaceManagerService();
            _blazorWorkspaceManager = new WorkspaceManagerService();

            _blazorWorkspaceManager.CreateSolutionFile();
            _webFormsWorkspaceManager.CreateSolutionFile();
        }
    }
}
