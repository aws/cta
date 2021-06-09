using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CTA.WebForms2Blazor.Factories;
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
        private WebFormsProjectAnalyzer _projectAnalayzer;
        private BlazorProjectBuilder _projectBuilder;
        private IEnumerable<FileInformation> _fileInformationCollection;

        public MigrationManager(string inputProjectPath, string outputProjectPath)
        {
            _inputProjectPath = inputProjectPath;
            _outputProjectPath = outputProjectPath;
        }

        public async void PerformMigration()
        {
            _projectAnalayzer = new WebFormsProjectAnalyzer(_inputProjectPath);
            _projectBuilder = new BlazorProjectBuilder(_outputProjectPath);
            _fileInformationCollection = FileInformationFactory.BuildMany(_projectAnalayzer.GetProjectFileInfo());

            var migrationTasks = _fileInformationCollection.Select(fileInformation =>
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
                        _projectBuilder.WriteFileInformationToProject(generatedFile);
                    }
                }));

            // Combines migration tasks into a single task we can await
            await Task.WhenAll(migrationTasks);

            // TODO: Any necessary cleanup or last checks on new project
        }
    }
}
