using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CTA.WebForms2Blazor.Services
{
    public class WorkspaceManagerService
    {
        private const string WorkspaceDuplicateError = "Attempted to create a new Blazor workspace when one already exists";
        private const string WorkspaceMissingeErrorTemplate = "Workspace {0} operation attempted, but no workspace exists";
        private const string WorkspaceUpdateFailedErrorTemplate = "Workspace {0} operation attempted, but workspace failed to apply changes";

        // We track number of projects and documents explicitly because the number of
        // documents in the workspace only equates to code files, and also for use by
        // functions or services that wait on conditions using these values. Continually
        // enumerating the project list and the individual document lists would add up
        // computationally during these waiting periods
        private int _numProjects;
        private int _numDocuments;
        private int _expectedProjects;
        private int _expectedDocuments;
        private AdhocWorkspace _workspace;

        public Solution CurrentSolution { get { return _workspace.CurrentSolution; } }

        public void CreateSolutionFile()
        {
            if (_workspace != null)
            {
                throw new InvalidOperationException(WorkspaceDuplicateError);
            }

            _workspace = new AdhocWorkspace();
            var solutionInfo = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Default);
            _workspace.AddSolution(solutionInfo);
        }

        public Project CreateProjectFile(string projectName, IEnumerable<ProjectReference> projectReferences = null, IEnumerable<MetadataReference> metadataReferences = null)
        {
            if (_workspace == null)
            {
                // I opted to just allow an on-the-spot creation of the workspace and solution
                // in this case as there isn't really a whole lot of risk to create a solution
                // and a workspace if they doesn't exist. The other functions throw exceptions
                // in this case because it signifies that processing is occurring before project
                // files have been sufficiently processed which is definitely bad

                CreateSolutionFile();
            }

            var projectInfo = ProjectInfo.Create(
                id: ProjectId.CreateNewId(),
                version: VersionStamp.Default,
                name: projectName,
                assemblyName: projectName,
                language: LanguageNames.CSharp,
                projectReferences: projectReferences,
                metadataReferences: metadataReferences
            );

            Project project = _workspace.AddProject(projectInfo);
            _numProjects += 1;

            return project;
        }

        public void AddProjectReferences(string projectName, IEnumerable<ProjectReference> projectReferences)
        {
            if (_workspace == null)
            {
                throw new InvalidOperationException(string.Format(WorkspaceMissingeErrorTemplate, "add project reference"));
            }

            var targetProject = _workspace.CurrentSolution.Projects.Where(project => project.Name.Equals(projectName)).Single();
            var newSolution = _workspace.CurrentSolution.WithProjectReferences(targetProject.Id, projectReferences);

            if (!_workspace.TryApplyChanges(newSolution))
            {
                throw new InvalidOperationException(string.Format(WorkspaceUpdateFailedErrorTemplate, "add project reference"));
            }
        }

        public void AddMetadataReferences(string projectName, IEnumerable<MetadataReference> metadataReferences)
        {
            if (_workspace == null)
            {
                throw new InvalidOperationException(string.Format(WorkspaceMissingeErrorTemplate, "add metadata reference"));
            }

            var targetProject = _workspace.CurrentSolution.Projects.Where(project => project.Name.Equals(projectName)).Single();
            var newSolution = _workspace.CurrentSolution.WithProjectMetadataReferences(targetProject.Id, metadataReferences);

            if (!_workspace.TryApplyChanges(newSolution))
            {
                throw new InvalidOperationException(string.Format(WorkspaceUpdateFailedErrorTemplate, "add metadata reference"));
            }
        }

        public Document AddDocument(string projectName, string documentName, string documentText)
        {
            if (_workspace == null)
            {
                throw new InvalidOperationException(string.Format(WorkspaceMissingeErrorTemplate, "add document"));
            }

            var targetProject = _workspace.CurrentSolution.Projects.Where(project => project.Name.Equals(projectName)).Single();
            Document document = _workspace.AddDocument(targetProject.Id, documentName, SourceText.From(documentText));
            _numDocuments += 1;

            return document;
        }

        public void NotifyNewExpectedProject()
        {
            _expectedProjects += 1;
        }

        public void NotifyNewExpectedDocument()
        {
            _expectedDocuments += 1;
        }

        public async Task WaitUntilAllProjectsInWorkspace(CancellationToken token)
        {
            while (_numProjects < _expectedProjects)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                await Task.Delay(25);
            }
        }

        public async Task WaitUntilAllDocumentsInWorkspace(CancellationToken token)
        {
            while (_numDocuments < _expectedDocuments)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                await Task.Delay(25);
            }
        }
    }
}
