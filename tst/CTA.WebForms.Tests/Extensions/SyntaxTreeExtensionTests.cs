using CTA.WebForms.Services;
using CTA.WebForms.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace CTA.WebForms.Tests.Extensions
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
        private const string TestStatementCommentShort = "This is a short comment";
        private const string TestClassName = "TestClass";
        private const string TestBaseClassName1 = "TestBaseClass1";
        private const string TestBaseClassName2 = "TestBaseClass2";
        private const string TestMethodName = "TestMethod";
        private const string TestMethodWrongName = "TestMethodWrong";
        private const string EventHandlerObjectParamType = "object";
        private const string EventHandlerObjectParamName = "sender";
        private const string EventHandlerEventArgsParamType = "EventArgs";
        private const string EventHandlerEventArgsParamName = "e";
        private const string TestExprNoParens = "new EventHandler(() => { })";
        private const string TestExprSingleParens = "(new EventHandler(() => { }))";
        private const string TestExprThreeParens = "(((new EventHandler(() => { }))))";

        private const string ClassWith1BaseType =
@"class TestClass : TestBaseClass1
{
}";
        private const string ClassWith2BaseTypes =
@"class TestClass : TestBaseClass1, TestBaseClass2
{
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
        public void UnionSyntaxNodeCollections_Counts_Duplicates_Regardless_Of_Trivia()
        {
            var statement1 = SyntaxFactory.ParseStatement(TestStatementText);
            var statement2 = statement1.AddComment(TestStatementCommentShort);

            var statementCollection1 = new[] { statement1 };
            var statementCollection2 = new[] { statement2 };

            Assert.AreEqual(statement1, statementCollection1.UnionSyntaxNodeCollections(statementCollection2).Single());
        }

        [Test]
        public void IsEventHandler_Returns_True_For_Correct_Event_Handler()
        {
            var methodType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            var methodName = SyntaxFactory.Identifier(TestMethodName);

            var param1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerObjectParamName))
                .WithType(SyntaxFactory.ParseTypeName(EventHandlerObjectParamType));
            var param2 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerEventArgsParamName))
                .WithType(SyntaxFactory.ParseTypeName(EventHandlerEventArgsParamType));

            var method = SyntaxFactory.MethodDeclaration(methodType, methodName)
                .AddParameterListParameters(param1, param2);

            Assert.True(method.IsEventHandler(TestMethodName));
        }

        [Test]
        public void IsEventHandler_Returns_False_For_Mismatched_Handler_Name()
        {
            var methodType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            var methodName = SyntaxFactory.Identifier(TestMethodWrongName);

            var param1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerObjectParamName))
                .WithType(SyntaxFactory.ParseTypeName(EventHandlerObjectParamType));
            var param2 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerEventArgsParamName))
                .WithType(SyntaxFactory.ParseTypeName(EventHandlerEventArgsParamType));

            var method = SyntaxFactory.MethodDeclaration(methodType, methodName)
                .AddParameterListParameters(param1, param2);

            Assert.False(method.IsEventHandler(TestMethodName));
        }

        [Test]
        public void IsEventHandler_Returns_False_For_Incorrect_Parameters()
        {
            var methodType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            var methodName = SyntaxFactory.Identifier(TestMethodName);

            var method = SyntaxFactory.MethodDeclaration(methodType, methodName);

            Assert.False(method.IsEventHandler(TestMethodName));
        }

        [Test]
        public void HasEventHandlerParameters_Returns_True_For_Correct_Params()
        {
            var methodType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            var methodName = SyntaxFactory.Identifier(TestMethodName);

            var param1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerObjectParamName))
                .WithType(SyntaxFactory.ParseTypeName(EventHandlerObjectParamType));
            var param2 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerEventArgsParamName))
                .WithType(SyntaxFactory.ParseTypeName(EventHandlerEventArgsParamType));

            var method = SyntaxFactory.MethodDeclaration(methodType, methodName)
                .AddParameterListParameters(param1, param2);

            Assert.True(method.HasEventHandlerParameters());
        }

        [Test]
        public void HasEventHandlerParameters_Returns_False_For_Incorrect_Params()
        {
            var methodType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            var methodName = SyntaxFactory.Identifier(TestMethodName);

            var param1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerObjectParamName))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var param2 = SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventHandlerEventArgsParamName))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            var method = SyntaxFactory.MethodDeclaration(methodType, methodName)
                .AddParameterListParameters(param1, param2);

            Assert.False(method.HasEventHandlerParameters());
        }

        [Test]
        public void AddBaseType_Correctly_Adds_New_Base_Type()
        {
            var actualClassText = SyntaxFactory
                .ClassDeclaration(TestClassName)
                .AddBaseType(TestBaseClassName1)
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual(ClassWith1BaseType, actualClassText);
        }

        [Test]
        public void AddBaseType_Does_Not_Override_Existing_Base_Types()
        {
            var actualClassText = SyntaxFactory
                .ClassDeclaration(TestClassName)
                .AddBaseType(TestBaseClassName1)
                .AddBaseType(TestBaseClassName2)
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual(ClassWith2BaseTypes, actualClassText);
        }

        [Test]
        public void ClearBaseTypes_Removes_Multiple_Types()
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(TestClassName).AddBaseListTypes(new[] {
                SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(TestBaseClassName1)),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(TestBaseClassName1))
            });

            Assert.AreEqual(0, classDeclaration.ClearBaseTypes().BaseList.Types.Count);
        }

        [Test]
        public void RemoveSurroundingParentheses_Does_Nothing_For_Non_Parenthesized_Expression()
        {
            var expectedExpr = SyntaxFactory.ParseExpression(TestExprNoParens);

            Assert.AreEqual(expectedExpr.ToString(), expectedExpr.RemoveSurroundingParentheses().ToString());
        }

        [Test]
        public void RemoveSurroundingParentheses_Removes_Single_Parentheses()
        {
            var expectedExpr = SyntaxFactory.ParseExpression(TestExprNoParens);
            var inputExpr = SyntaxFactory.ParseExpression(TestExprSingleParens);

            Assert.AreEqual(expectedExpr.ToString(), inputExpr.RemoveSurroundingParentheses().ToString());
        }

        [Test]
        public void RemoveSurroundingParentheses_Removes_Multiple_Parentheses()
        {
            var expectedExpr = SyntaxFactory.ParseExpression(TestExprNoParens);
            var inputExpr = SyntaxFactory.ParseExpression(TestExprThreeParens);

            Assert.AreEqual(expectedExpr.ToString(), inputExpr.RemoveSurroundingParentheses().ToString());
        }
    }
}
