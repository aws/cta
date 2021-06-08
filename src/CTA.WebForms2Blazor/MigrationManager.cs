using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor
{
    public class MigrationManager
    {
        private readonly WebFormsProjectAnalyzer _projectAnalayzer;
        private readonly BlazorProjectBuilder _projectBuilder;
        private readonly IEnumerable<FileInformation> _fileInformationCollection;

        public MigrationManager(string inputProjectPath, string outputProjectPath)
        {
            _projectAnalayzer = new WebFormsProjectAnalyzer(inputProjectPath);
            _projectBuilder = new BlazorProjectBuilder(outputProjectPath);

            _fileInformationCollection = FileInformationFactory.BuildMany(_projectAnalayzer.GetProjectFileInfo());
        }

        public void PerformMigration()
        {
            // Central migration logic goes here
        }
    }
}
