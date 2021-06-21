using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.Services
{
    public class WorkspaceManagerServiceTests
    {
        private const string TestDocument1Text =
@"namespace TestNamespace1 {
    public class TestClass1 {
        public void TestMethod1 () { }
    }
}";

        private WorkspaceManagerService _workspaceManager;

        [SetUp]
        public void SetUp()
        {
            _workspaceManager = new WorkspaceManagerService();
        }

        [Test]
        public void CreateSolutionFile_Creates_Workspace_And_Solution()
        {
            _workspaceManager.CreateSolutionFile();

            Assert.NotNull(_workspaceManager.CurrentSolution);
        }

        [Test]
        public void CreateSolutionFile_Throws_Exception_On_Second_Call()
        {
            _workspaceManager.CreateSolutionFile();

            Assert.Throws(typeof(InvalidOperationException), () => _workspaceManager.CreateSolutionFile());
        }

        [Test]
        public void CreateProjectFile_Creates_Single_Project_On_Solution()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.CreateProjectFile("TestProjectName");

            Assert.AreEqual(_workspaceManager.CurrentSolution.Projects.Count(), 1);
        }

        [Test]
        public void CreateProjectFile_Creates_Solution_If_None_Exists()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.CreateProjectFile("TestProjectName");

            Assert.NotNull(_workspaceManager.CurrentSolution);
            Assert.AreEqual(_workspaceManager.CurrentSolution.Projects.Count(), 1);
        }

        [Test]
        public void CreateProjectFile_Works_For_Multiple_Projects()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.CreateProjectFile("TestProjectName1");
            _workspaceManager.CreateProjectFile("TestProjectName2");
            _workspaceManager.CreateProjectFile("TestProjectName3");

            Assert.AreEqual(_workspaceManager.CurrentSolution.Projects.Count(), 3);
        }

        [Test]
        public void WaitUntilAllProjectsInWorkspace_Completes_When_All_Projects_Completed()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();

            var task = _workspaceManager.WaitUntilAllProjectsInWorkspace(token);
            Assert.False(task.IsCompleted);

            _workspaceManager.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceManager.CreateProjectFile("TestProjectName2");
            Assert.False(task.IsCompleted);

            _workspaceManager.CreateProjectFile("TestProjectName3");
            Assert.DoesNotThrowAsync(async () => await task);
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Test]
        public void WaitUntilAllProjectsInWorkspace_Task_Reflects_Cancellations()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();

            var task = _workspaceManager.WaitUntilAllProjectsInWorkspace(token);
            Assert.False(task.IsCompleted);

            _workspaceManager.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            source.Cancel();
            Assert.ThrowsAsync(typeof(TaskCanceledException), async () => await task);
            Assert.True(task.IsCanceled);
        }

        [Test]
        public void AddDocument_Adds_Single_Document_To_Project()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var pid2 = _workspaceManager.CreateProjectFile("TestProjectName2");
            _workspaceManager.AddDocument(pid2, "TestDocumentName", "");

            var project1 = _workspaceManager.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName1")).Single();
            var project2 = _workspaceManager.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName2")).Single();

            Assert.AreEqual(project1.Documents.Count(), 0);
            Assert.AreEqual(project2.Documents.Count(), 1);
        }

        [Test]
        public void AddDocument_Works_For_Multiple_Documents()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            _workspaceManager.NotifyNewExpectedDocument();
            _workspaceManager.NotifyNewExpectedDocument();
            _workspaceManager.NotifyNewExpectedDocument();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var pid2 = _workspaceManager.CreateProjectFile("TestProjectName2");
            _workspaceManager.AddDocument(pid1, "TestDocumentName1", "");
            _workspaceManager.AddDocument(pid2, "TestDocumentName2", "");
            _workspaceManager.AddDocument(pid2, "TestDocumentName3", "");
            _workspaceManager.AddDocument(pid2, "TestDocumentName4", "");

            var project1 = _workspaceManager.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName1")).Single();
            var project2 = _workspaceManager.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName2")).Single();

            Assert.AreEqual(project1.Documents.Count(), 1);
            Assert.AreEqual(project2.Documents.Count(), 3);
        }

        [Test]
        public void AddDocument_Throws_Exception_On_Invalid_Project_Id()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            Assert.Throws(typeof(ArgumentException), () => _workspaceManager.AddDocument(ProjectId.CreateNewId(), "TestDocumentName", ""));
        }

        [Test]
        public void WaitUntilAllDocumentsInWorkspace_Completes_When_All_Documents_Added()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            _workspaceManager.NotifyNewExpectedDocument();

            var task = _workspaceManager.WaitUntilAllDocumentsInWorkspace(token);
            Assert.False(task.IsCompleted);

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceManager.AddDocument(pid1, "TestDocumentName1", "");
            Assert.False(task.IsCompleted);

            _workspaceManager.AddDocument(pid1, "TestDocumentName2", "");
            Assert.DoesNotThrowAsync(async () => await task);
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Test]
        public void WaitUntilAllDocumentsInWorkspace_Task_Reflects_Cancellations()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            _workspaceManager.NotifyNewExpectedDocument();

            var task = _workspaceManager.WaitUntilAllDocumentsInWorkspace(token);
            Assert.False(task.IsCompleted);

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceManager.AddDocument(pid1, "TestDocumentName1", "");
            Assert.False(task.IsCompleted);

            source.Cancel();
            Assert.ThrowsAsync(typeof(TaskCanceledException), async () => await task);
            Assert.True(task.IsCanceled);
        }

        [Test]
        public async Task GetCurrentDocumentSyntaxTree_Correctly_Builds_Syntax_Tree()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            var syntaxTree = await _workspaceManager.GetCurrentDocumentSyntaxTree(did1);

            Assert.AreEqual(TestDocument1Text, syntaxTree.ToString());
        }

        [Test]
        public void GetCurrentDocumentSyntaxTree_Throws_Exception_On_Invalid_Document_Id()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            Assert.ThrowsAsync(typeof(ArgumentException), async () => await _workspaceManager.GetCurrentDocumentSyntaxTree(DocumentId.CreateNewId(pid1)));
        }

        [Test]
        public async Task GetCurrentDocumentSemanticModel_Builds_Model_With_Necessary_Symbols()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);

            var rootNode = model.SyntaxTree.GetRoot();
            var descendantNodes = rootNode.DescendantNodes();
            var classDeclaration = descendantNodes.OfType<ClassDeclarationSyntax>().Single();
            var methodDeclaration = descendantNodes.OfType<MethodDeclarationSyntax>().Single();

            var classSymbol = model.GetDeclaredSymbol(classDeclaration);
            var methodSymbol = model.GetDeclaredSymbol(methodDeclaration);

            Assert.AreEqual(classSymbol.Name, "TestClass1");
            Assert.AreEqual(methodSymbol.Name, "TestMethod1");
        }

        [Test]
        public void GetCurrentDocumentSemanticModel_Throws_Exception_On_Invalid_Document_Id()
        {
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            Assert.ThrowsAsync(typeof(ArgumentException), async () => await _workspaceManager.GetCurrentDocumentSemanticModel(DocumentId.CreateNewId(pid1)));
        }
    }
}
