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

        // Do these need to be class fields?
        private WorkspaceManagerService _webFormsWorkspaceBuilder;
        private WorkspaceManagerService _blazorWorkspaceBuilder;
        private ProjectAnalyzer _webFormsProjectAnalyzer;
        private ProjectBuilder _blazorProjectBuilder;

        public MigrationManager(string inputProjectPath, string outputProjectPath)
        {
            _inputProjectPath = inputProjectPath;
            _outputProjectPath = outputProjectPath;
        }

        public async Task PerformMigration()
        {
            SetUpWorkspaceBuilders();

            _webFormsProjectAnalyzer = new ProjectAnalyzer(_inputProjectPath);
            _blazorProjectBuilder = new ProjectBuilder(_outputProjectPath);

            // Pass workspace build manager to factory constructor

            var fileInformationCollection = FileInformationFactory.BuildMany(_webFormsProjectAnalyzer.GetProjectFileInfo());

            var migrationTasks = fileInformationCollection.Select(fileInformation =>
                // ContinueWith specifies the action to be run after each task completes,
                // in this case it sends each generated file to the project builder
                fileInformation.MigrateFileAsync().ContinueWith(generatedFiles =>
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

        private void SetUpWorkspaceBuilders()
        {
            _webFormsWorkspaceBuilder = new WorkspaceManagerService();
            _blazorWorkspaceBuilder = new WorkspaceManagerService();

            _blazorWorkspaceBuilder.CreateSolutionFile();
            _webFormsWorkspaceBuilder.CreateSolutionFile();
        }
    }
}
