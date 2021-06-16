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
        private const string WorkspaceMissingeErrorTemplate = "Attempted {0} operation, but no workspace exists";
        private const string ProjectMissingErrorTemplate = "Attempted {0} operation, but required project [id:{1}] does not exist";
        private const string DocumentMissingErrorTemplate = "Attempted {0} operation, but required document [id:{1}] does not exist";
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

        public ProjectId CreateProjectFile(string projectName, IEnumerable<ProjectReference> projectReferences = null, IEnumerable<MetadataReference> metadataReferences = null)
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

            return project.Id;
        }

        public void AddProjectReferences(ProjectId projectId, IEnumerable<ProjectReference> projectReferences)
        {
            ThrowErrorIfProjectNotExists("add project reference");

            var targetProject = GetProjectById(projectId, "add project reference");
            var newSolution = _workspace.CurrentSolution.WithProjectReferences(targetProject.Id, projectReferences);

            ApplyWorkspaceChanges(newSolution, "add project reference");
        }

        public void AddMetadataReferences(ProjectId projectId, IEnumerable<MetadataReference> metadataReferences)
        {
            ThrowErrorIfProjectNotExists("add metadata reference");

            var targetProject = GetProjectById(projectId, "add metadata reference");
            var newSolution = _workspace.CurrentSolution.WithProjectMetadataReferences(targetProject.Id, metadataReferences);

            ApplyWorkspaceChanges(newSolution, "add metadata reference");
        }

        public DocumentId AddDocument(ProjectId projectId, string documentName, string documentText)
        {
            ThrowErrorIfProjectNotExists("add document");

            var targetProject = GetProjectById(projectId, "add document");
            Document document = _workspace.AddDocument(targetProject.Id, documentName, SourceText.From(documentText));
            _numDocuments += 1;

            return document.Id;
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

        public async Task<SyntaxTree> GetCurrentDocumentSyntaxTree(DocumentId documentId)
        {
            var document = GetDocumentById(documentId, "get syntax tree");

            return await document.GetSyntaxTreeAsync();
        }

        public async Task<SemanticModel> GetCurrentDocumentSemanticModel(DocumentId documentId)
        {
            var document = GetDocumentById(documentId, "get semantic model");

            return await document.GetSemanticModelAsync();
        }

        private void ThrowErrorIfProjectNotExists(string operation)
        {
            if (_workspace == null)
            {
                throw new InvalidOperationException(string.Format(WorkspaceMissingeErrorTemplate, operation));
            }
        }

        private void ApplyWorkspaceChanges(Solution solution, string operation)
        {
            if (!_workspace.TryApplyChanges(solution))
            {
                throw new InvalidOperationException(string.Format(WorkspaceUpdateFailedErrorTemplate, operation));
            }
        }

        private Project GetProjectById(ProjectId projectId, string operation)
        {
            var project = _workspace.CurrentSolution.GetProject(projectId);

            if (project == null)
            {
                throw new ArgumentException(string.Format(ProjectMissingErrorTemplate, operation, projectId.Id));
            }

            return project;
        }

        private Document GetDocumentById(DocumentId documentId, string operation)
        {
            var document = _workspace.CurrentSolution.GetDocument(documentId);

            if (document == null)
            {
                throw new ArgumentException(string.Format(DocumentMissingErrorTemplate, operation, documentId.Id));
            }

            return document;
        }
    }
}
