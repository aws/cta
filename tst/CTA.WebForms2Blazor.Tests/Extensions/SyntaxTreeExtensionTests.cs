using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

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
        private const string TestStatementText = "var x = 10;";
        private const string TestCommentPrependChars = "// ";
        private const string TestStatementCommentShort = "This is a short comment";

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
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

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
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

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

        [Test]
        public void AddComment_Prepends_Text_To_Node_With_Double_Slash()
        {
            var expectedOutput = $"{TestCommentPrependChars}{TestStatementCommentShort}\r\n{TestStatementText}";
            var statement = SyntaxFactory.ParseStatement(TestStatementText);
            statement = statement.AddComment(new [] { TestStatementCommentShort });

            Assert.AreEqual(expectedOutput, statement.NormalizeWhitespace().ToFullString());
        }

        [TestCase("Single", 10, new[] { "Single" })]
        [TestCase("Double Double", 6, new[] { "Double", "Double" })]
        [TestCase("Four Four Four Four", 8, new[] { "Four Four", "Four Four" })]
        [TestCase("  This     is a long,   long, long,  long    comment ", 9, new[] { "This is a", "long, long,", "long, long", "comment" })]
        public void AddComment_Splits_Text_Before_Prepending_With_Double_Slash(string comment, int splitSize, IEnumerable<string> commentParts)
        {
            string expectedComment = string.Empty;
            foreach (var commentPart in commentParts)
            {
                expectedComment += $"{TestCommentPrependChars}{commentPart}\r\n";
            }

            var expectedOutput = expectedComment + TestStatementText;
            var statement = SyntaxFactory.ParseStatement(TestStatementText);
            // Set soft character limit to over length of test statement to prevent line breaking
            statement = statement.AddComment(comment, splitSize);

            Assert.AreEqual(expectedOutput, statement.NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void UnionSyntaxNodeCollections_Counts_Duplicates_Regardless_Of_Trivia()
        {
            var statement1 = SyntaxFactory.ParseStatement(TestStatementText);
            var statement2 = statement1.AddComment(TestStatementCommentShort);

            var statementCollection1 = new[] { statement1 };
            var statementCollection2 = new[] { statement2 };

            Assert.AreEqual(statement1, statementCollection1.UnionSyntaxNodeCollections(statementCollection2).Single());
        }
    }
}
