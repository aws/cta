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

        private WorkspaceManagerService _workspaceBuilder;

        [SetUp]
        public void SetUp()
        {
            _workspaceBuilder = new WorkspaceManagerService();
        }

        [Test]
        public void CreateSolutionFile_Creates_Workspace_And_Solution()
        {
            _workspaceBuilder.CreateSolutionFile();

            Assert.NotNull(_workspaceBuilder.CurrentSolution);
        }

        [Test]
        public void CreateSolutionFile_Throws_Exception_On_Second_Call()
        {
            _workspaceBuilder.CreateSolutionFile();

            Assert.Throws(typeof(InvalidOperationException), () => _workspaceBuilder.CreateSolutionFile());
        }

        [Test]
        public void CreateProjectFile_Creates_Single_Project_On_Solution()
        {
            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.CreateProjectFile("TestProjectName");

            Assert.AreEqual(_workspaceBuilder.CurrentSolution.Projects.Count(), 1);
        }

        [Test]
        public void CreateProjectFile_Creates_Solution_If_None_Exists()
        {
            _workspaceBuilder.CreateProjectFile("TestProjectName");

            Assert.NotNull(_workspaceBuilder.CurrentSolution);
            Assert.AreEqual(_workspaceBuilder.CurrentSolution.Projects.Count(), 1);
        }

        [Test]
        public void CreateProjectFile_Works_For_Multiple_Projects()
        {
            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            _workspaceBuilder.CreateProjectFile("TestProjectName2");
            _workspaceBuilder.CreateProjectFile("TestProjectName3");

            Assert.AreEqual(_workspaceBuilder.CurrentSolution.Projects.Count(), 3);
        }

        [Test]
        public void WaitUntilAllProjectsInWorkspace_Completes_When_All_Projects_Completed()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.NotifyNewExpectedProject();
            _workspaceBuilder.NotifyNewExpectedProject();
            _workspaceBuilder.NotifyNewExpectedProject();

            var task = _workspaceBuilder.WaitUntilAllProjectsInWorkspace(token);
            Assert.False(task.IsCompleted);

            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.CreateProjectFile("TestProjectName2");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.CreateProjectFile("TestProjectName3");
            Assert.DoesNotThrowAsync(async () => await task);
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Test]
        public void WaitUntilAllProjectsInWorkspace_Task_Reflects_Cancellations()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.NotifyNewExpectedProject();
            _workspaceBuilder.NotifyNewExpectedProject();

            var task = _workspaceBuilder.WaitUntilAllProjectsInWorkspace(token);
            Assert.False(task.IsCompleted);

            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            source.Cancel();
            Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await task);
            Assert.True(task.IsCanceled);
        }

        [Test]
        public void AddDocument_Adds_Single_Document_To_Project()
        {
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            var pid2 = _workspaceBuilder.CreateProjectFile("TestProjectName2");
            _workspaceBuilder.AddDocument(pid2, "TestDocumentName", "");

            var project1 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName1")).Single();
            var project2 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName2")).Single();

            Assert.AreEqual(project1.Documents.Count(), 0);
            Assert.AreEqual(project2.Documents.Count(), 1);
        }

        [Test]
        public void AddDocument_Works_For_Multiple_Documents()
        {
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            var pid2 = _workspaceBuilder.CreateProjectFile("TestProjectName2");
            _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", "");
            _workspaceBuilder.AddDocument(pid2, "TestDocumentName2", "");
            _workspaceBuilder.AddDocument(pid2, "TestDocumentName3", "");
            _workspaceBuilder.AddDocument(pid2, "TestDocumentName4", "");

            var project1 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName1")).Single();
            var project2 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName2")).Single();

            Assert.AreEqual(project1.Documents.Count(), 1);
            Assert.AreEqual(project2.Documents.Count(), 3);
        }

        [Test]
        public void AddDocument_Throws_Exception_On_Invalid_Project_Id()
        {
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.Throws(typeof(ArgumentException), () => _workspaceBuilder.AddDocument(ProjectId.CreateNewId(), "TestDocumentName", ""));
        }

        [Test]
        public void WaitUntilAllDocumentsInWorkspace_Completes_When_All_Documents_Added()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.NotifyNewExpectedProject();
            _workspaceBuilder.NotifyNewExpectedDocument();
            _workspaceBuilder.NotifyNewExpectedDocument();

            var task = _workspaceBuilder.WaitUntilAllDocumentsInWorkspace(token);
            Assert.False(task.IsCompleted);

            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", "");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.AddDocument(pid1, "TestDocumentName2", "");
            Assert.DoesNotThrowAsync(async () => await task);
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Test]
        public void WaitUntilAllDocumentsInWorkspace_Task_Reflects_Cancellations()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.NotifyNewExpectedProject();
            _workspaceBuilder.NotifyNewExpectedDocument();
            _workspaceBuilder.NotifyNewExpectedDocument();

            var task = _workspaceBuilder.WaitUntilAllDocumentsInWorkspace(token);
            Assert.False(task.IsCompleted);

            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", "");
            Assert.False(task.IsCompleted);

            source.Cancel();
            Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await task);
            Assert.True(task.IsCanceled);
        }

        [Test]
        public async Task GetCurrentDocumentSyntaxTree_Correctly_Builds_Syntax_Tree()
        {
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            var syntaxTree = await _workspaceBuilder.GetCurrentDocumentSyntaxTree(did1);

            Assert.AreEqual(TestDocument1Text, syntaxTree.ToString());
        }

        [Test]
        public void GetCurrentDocumentSyntaxTree_Throws_Exception_On_Invalid_Document_Id()
        {
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            Assert.ThrowsAsync(typeof(ArgumentException), async () => await _workspaceBuilder.GetCurrentDocumentSyntaxTree(DocumentId.CreateNewId(pid1)));
        }

        [Test]
        public async Task GetCurrentDocumentSemanticModel_Builds_Model_With_Necessary_Symbols()
        {
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            var model = await _workspaceBuilder.GetCurrentDocumentSemanticModel(did1);

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
            _workspaceBuilder.CreateSolutionFile();
            var pid1 = _workspaceBuilder.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceBuilder.AddDocument(pid1, "TestDocumentName1", TestDocument1Text);

            Assert.ThrowsAsync(typeof(ArgumentException), async () => await _workspaceBuilder.GetCurrentDocumentSemanticModel(DocumentId.CreateNewId(pid1)));
        }
    }
}
