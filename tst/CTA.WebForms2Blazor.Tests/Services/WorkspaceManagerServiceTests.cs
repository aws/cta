using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace CTA.WebForms2Blazor.Tests.Services
{
    public class WorkspaceManagerServiceTests
    {
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
            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            _workspaceBuilder.CreateProjectFile("TestProjectName2");
            _workspaceBuilder.AddDocument("TestProjectName2", "TestDocumentName", "");

            var project1 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName1")).Single();
            var project2 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName2")).Single();

            Assert.AreEqual(project1.Documents.Count(), 0);
            Assert.AreEqual(project2.Documents.Count(), 1);
        }

        [Test]
        public void AddDocument_Works_For_Multiple_Documents()
        {
            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            _workspaceBuilder.CreateProjectFile("TestProjectName2");
            _workspaceBuilder.AddDocument("TestProjectName1", "TestDocumentName1", "");
            _workspaceBuilder.AddDocument("TestProjectName2", "TestDocumentName2", "");
            _workspaceBuilder.AddDocument("TestProjectName2", "TestDocumentName3", "");
            _workspaceBuilder.AddDocument("TestProjectName2", "TestDocumentName4", "");

            var project1 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName1")).Single();
            var project2 = _workspaceBuilder.CurrentSolution.Projects.Where(project => project.Name.Equals("TestProjectName2")).Single();

            Assert.AreEqual(project1.Documents.Count(), 1);
            Assert.AreEqual(project2.Documents.Count(), 3);
        }

        [Test]
        public void AddDocument_Throws_Exception_If_Project_Doesnt_Exist()
        {
            _workspaceBuilder.CreateSolutionFile();
            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.Throws(typeof(InvalidOperationException), () => _workspaceBuilder.AddDocument("TestProjectName2", "TestDocumentName", ""));
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

            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.AddDocument("TestProjectName1", "TestDocumentName1", "");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.AddDocument("TestProjectName1", "TestDocumentName2", "");
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

            _workspaceBuilder.CreateProjectFile("TestProjectName1");
            Assert.False(task.IsCompleted);

            _workspaceBuilder.AddDocument("TestProjectName1", "TestDocumentName1", "");
            Assert.False(task.IsCompleted);

            source.Cancel();
            Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await task);
            Assert.True(task.IsCanceled);
        }
    }
}
