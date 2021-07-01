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
        private const string WorkspaceTooManyOperationsError = "Attempted {0} operation, but the expected number of these operations has been reached";
        private const string ProjectMissingErrorTemplate = "Attempted {0} operation, but required project [id:{1}] does not exist";
        private const string DocumentMissingErrorTemplate = "Attempted {0} operation, but required document [id:{1}] does not exist";
        private const string WorkspaceUpdateFailedErrorTemplate = "Workspace {0} operation attempted, but workspace failed to apply changes";

        private const string AddProjectReferenceOperation = "add project references";
        private const string AddMetadataReferenceOperation = "add metadata reference";
        private const string AddDocumentOperation = "add document";
        private const string AddProjectOperation = "add project";
        private const string GetSyntaxTreeOperation = "get syntax tree";
        private const string GetSemanticModelOperation = "get semantic model";

        // We track number of projects and documents explicitly because the number of
        // documents in the workspace only equates to code files, and also for use by
        // functions or services that wait on conditions using these values. Continually
        // enumerating the project list and the individual document lists would add up
        // computationally during these waiting periods
        private int _numProjects;
        private int _numDocuments;
        private int _expectedProjects;
        private int _expectedDocuments;
        // Have to use bool because non-generic TaskCompletionSource not supported in
        // netcore3.1
        private List<TaskCompletionSource<bool>> _allProjectsInWorkspaceTaskSources;
        private List<TaskCompletionSource<bool>> _allDocumentsInWorkspaceTaskSources;
        private AdhocWorkspace _workspace;

        public Solution CurrentSolution { get { return _workspace.CurrentSolution; } }

        public WorkspaceManagerService()
        {
            _allProjectsInWorkspaceTaskSources = new List<TaskCompletionSource<bool>>();
            _allDocumentsInWorkspaceTaskSources = new List<TaskCompletionSource<bool>>();
        }

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

            if (_numProjects == _expectedProjects)
            {
                _allProjectsInWorkspaceTaskSources.ForEach(source =>
                {
                    // A cancelled task also counts as completed
                    if (!source.Task.IsCompleted)
                    {
                        source.SetResult(true);
                    }
                });
            }
            else if (_numProjects > _expectedProjects)
            {
                throw new InvalidOperationException(string.Format(WorkspaceTooManyOperationsError, AddProjectOperation));
            }

            return project.Id;
        }

        public void AddProjectReferences(ProjectId projectId, IEnumerable<ProjectReference> projectReferences)
        {
            ThrowErrorIfProjectNotExists(AddProjectReferenceOperation);

            var targetProject = GetProjectById(projectId, AddProjectReferenceOperation);
            var newSolution = _workspace.CurrentSolution.WithProjectReferences(targetProject.Id, projectReferences);

            ApplyWorkspaceChanges(newSolution, AddProjectReferenceOperation);
        }

        public void AddMetadataReferences(ProjectId projectId, IEnumerable<MetadataReference> metadataReferences)
        {
            ThrowErrorIfProjectNotExists(AddMetadataReferenceOperation);

            var targetProject = GetProjectById(projectId, AddMetadataReferenceOperation);
            var newSolution = _workspace.CurrentSolution.WithProjectMetadataReferences(targetProject.Id, metadataReferences);

            ApplyWorkspaceChanges(newSolution, AddMetadataReferenceOperation);
        }

        // I decided to add a new method because I wasn't sure how necessary the
        // instantaneous wait really was for this service, we can remove the other
        // one later if we decide we only need the async version
        public async Task<DocumentId> AddDocumentAsync(ProjectId projectId, string documentName, string documentText, CancellationToken token)
        {
            await WaitUntilAllProjectsInWorkspace(token);
            return AddDocument(projectId, documentName, documentText);
        }

        public DocumentId AddDocument(ProjectId projectId, string documentName, string documentText)
        {
            ThrowErrorIfProjectNotExists(AddDocumentOperation);

            var targetProject = GetProjectById(projectId, AddDocumentOperation);
            Document document = _workspace.AddDocument(targetProject.Id, documentName, SourceText.From(documentText));
            _numDocuments += 1;

            if (_numDocuments == _expectedDocuments)
            {
                _allDocumentsInWorkspaceTaskSources.ForEach(source =>
                {
                    // A cancelled task also counts as completed
                    if (!source.Task.IsCompleted)
                    {
                        source.SetResult(true);
                    }
                });
            }
            else if (_numDocuments > _expectedDocuments)
            {
                throw new InvalidOperationException(string.Format(WorkspaceTooManyOperationsError, AddDocumentOperation));
            }

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

        public Task<bool> WaitUntilAllProjectsInWorkspace(CancellationToken token)
        {
            var source = new TaskCompletionSource<bool>();

            if (_numProjects >= _expectedProjects)
            {
                source.SetResult(true);
            }
            else
            {
                token.Register(() => source.SetCanceled());
                _allProjectsInWorkspaceTaskSources.Add(source);
            }

            return source.Task;
        }

        public Task<bool> WaitUntilAllDocumentsInWorkspace(CancellationToken token)
        {
            var source = new TaskCompletionSource<bool>();

            if (_numDocuments >= _expectedDocuments)
            {
                source.SetResult(true);
            }
            else
            {
                token.Register(() => source.SetCanceled());
                _allDocumentsInWorkspaceTaskSources.Add(source);
            }

            return source.Task;
        }

        public async Task<SyntaxTree> GetCurrentDocumentSyntaxTree(DocumentId documentId)
        {
            var document = GetDocumentById(documentId, GetSyntaxTreeOperation);

            return await document.GetSyntaxTreeAsync();
        }

        public async Task<SemanticModel> GetCurrentDocumentSemanticModel(DocumentId documentId)
        {
            var document = GetDocumentById(documentId, GetSemanticModelOperation);

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
