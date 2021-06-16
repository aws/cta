using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Tests.Extensions
{
    public class SyntaxTreeExtensionTests
    {
        private const string TestDocumentInnerClassText =
            @"namespace TestNamespace1 {
                public class TestType1 {
                    public class TestType2 { }
                }
            }";

        private WorkspaceManagerService _workspaceManager;

        [SetUp]
        public void SetUp()
        {
            _workspaceManager = new WorkspaceManagerService();
            _workspaceManager.CreateSolutionFile();
        }

        [TestCase(
            @"namespace TestNamespace1 {
                public class TestType1 { }
                public abstract class TestType2 { }
            }")]
        [TestCase(
            @"namespace TestNamespace1 {
                public interface TestType1 { }
                public interface TestType2 { }
            }")]
        [TestCase(
            @"namespace TestNamespace1 {
                public class TestType1 { }
            }
            namespace TestNamespace2 {
                public class TestType2 { }
            }")]
        public async Task GetNamespaceLevelTypes_Retrieves_All_Types(string testDocumentText)
        {
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", testDocumentText);

            var syntaxTree = await _workspaceManager.GetCurrentDocumentSyntaxTree(did1);
            var testTypeNodes = syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>();
            var testType1 = testTypeNodes.Single(node => node.Identifier.ToString().Equals("TestType1"));
            var testType2 = testTypeNodes.Single(node => node.Identifier.ToString().Equals("TestType2"));

            var types = syntaxTree.GetNamespaceLevelTypes();
            
            Assert.True(types.Contains(testType1));
            Assert.True(types.Contains(testType2));
        }

        [Test]
        public async Task GetNamespaceLevelTypes_Ignores_Inner_Classes()
        {
            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocumentInnerClassText);

            var syntaxTree = await _workspaceManager.GetCurrentDocumentSyntaxTree(did1);
            var testTypeNodes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var testType1 = testTypeNodes.Single(node => node.Identifier.ToString().Equals("TestType1"));
            var testType2 = testTypeNodes.Single(node => node.Identifier.ToString().Equals("TestType2"));

            var types = syntaxTree.GetNamespaceLevelTypes();

            Assert.True(types.Contains(testType1));
            Assert.False(types.Contains(testType2));
        }
    }
}
