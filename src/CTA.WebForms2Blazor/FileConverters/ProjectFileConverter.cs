using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;
using CTA.Rules.Config;
using CTA.Rules.ProjectFile;
using CTA.Rules.Update;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Metrics;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ProjectFileConverter : FileConverter
    {
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly ProjectAnalyzer _projectAnalyzer;
        private readonly WebFormMetricContext _metricsContext;
        private const string ChildActionType = "ProjectFileConverter";

        public ProjectFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer projectAnalyzer,
            TaskManagerService taskManagerService,
            WebFormMetricContext metricsContext
        ) : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _projectAnalyzer = projectAnalyzer;
            _metricsContext = metricsContext;

            _blazorWorkspaceManager.NotifyNewExpectedProject();
        }

        private string GenerateProjectFileContents(ProjectResult projectResult, 
            ProjectConfiguration projectConfiguration, List<String> projectReferences, List<String> metaReferences)
        {
            var projectActions = projectResult.ProjectActions;
            var packages = projectActions.PackageActions.Distinct()
                .ToDictionary(p => p.Name, p => p.Version);

            // Now we can finally create the ProjectFileCreator and use it
            var projectFileCreator = new ProjectFileCreator(FullPath, projectConfiguration.TargetVersions, packages, 
                projectReferences, ProjectType.WebForms, metaReferences);

            return projectFileCreator.CreateContents();
        }


        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectFileConversionMetrics(ChildActionType);

            // TODO: Extract info from project files and
            // call _blazorWorkspaceManager.CreateProjectFile
            // and _webFormsWorkspaceManager.CreateProjectFile

            // TODO: Retrieve cancellation token from thread manager service
            // _blazorWorkspaceManager.WaitUntilAllDocumentsInWorkspace(token);

            // TODO: Extract accumulated project info from 
            // workspace and use it to build the actual
            // project file

            string newCsProjContent = GenerateProjectFileContents(_projectAnalyzer.ProjectResult,
                _projectAnalyzer.ProjectConfiguration, _projectAnalyzer.ProjectReferences, _projectAnalyzer.MetaReferences);
            
            FileInformation fi = new FileInformation(FilePathHelper.RemoveDuplicateDirectories(RelativePath), Encoding.UTF8.GetBytes(newCsProjContent));

            var fileList = new List<FileInformation>() { fi };

            DoCleanUp();
            LogEnd();

            return Task.FromResult((IEnumerable<FileInformation>)fileList);
        }
    }
}
