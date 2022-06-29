using CTA.Rules.Actions.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;
using CTA.Rules.Models.Actions.VisualBasic;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class MethodBlockActionsTests
    {
        private MethodBlockActions _methodBlockActions;
        private SyntaxGenerator _syntaxGenerator;
        private MethodBlockSyntax _subNode;
        private MethodBlockSyntax _functionNode;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _methodBlockActions = new MethodBlockActions();

            var body = new SyntaxList<StatementSyntax>().Add(
                SyntaxFactory.ParseExecutableStatement(@"' Nothing to see here"));
            _subNode = SyntaxFactory.MethodBlock(SyntaxKind.SubBlock,
                SyntaxFactory
                    .MethodStatement(SyntaxKind.SubStatement, SyntaxFactory.Token(SyntaxKind.SubKeyword), "Authorize")
                    .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))),
                SyntaxFactory.EndSubStatement()).WithStatements(body);

            body = body.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression("0")));
            _functionNode = SyntaxFactory.MethodBlock(SyntaxKind.FunctionBlock,
                SyntaxFactory.MethodStatement(SyntaxKind.FunctionStatement,
                        SyntaxFactory.Token(SyntaxKind.FunctionKeyword), "TestFunction")
                    .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithAsClause(SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName("Integer"))),
                SyntaxFactory.EndFunctionStatement()).WithStatements(body);
        }

        [Test]
        public void MethodDeclarationAddComment()
        {
            const string comment = "This is a comment";
            var addCommentFunction = _methodBlockActions.GetAddCommentAction(comment);
            var newNode = addCommentFunction(_syntaxGenerator, _subNode);

            StringAssert.Contains(comment, newNode.ToFullString());
        }

        [Test]
        public void MethodDeclarationAddExpression()
        {
            var nodeBody = _subNode.Statements;
            nodeBody = nodeBody.Add(SyntaxFactory.ParseExecutableStatement("Dim testing as String = \"Testing\""));
            _subNode = _subNode.WithStatements(nodeBody);

            const string expression = "Dim i as Integer = 5";
            var addExpressionFunction = _methodBlockActions.GetAppendExpressionAction(expression);
            var newNode = addExpressionFunction(_syntaxGenerator, _subNode);

            StringAssert.Contains(expression, newNode.ToFullString());
        }

        [Test]
        public void MethodBlockActionComparison()
        {
            var methodDeclarationAction = new MethodBlockAction()
            {
                Key = "Test",
                Value = "Test2",
                MethodBlockActionFunc = _methodBlockActions.GetAddCommentAction("NewAttribute")
            };

            var cloned = methodDeclarationAction.Clone<MethodBlockAction>();

            Assert.True(methodDeclarationAction.Equals(cloned));
            cloned.Value = "DifferentValue";
            Assert.False(methodDeclarationAction.Equals(cloned));
        }

        [Test]
        public void CommentMethodAction()
        {
            var expressions = new List<string>
            {
                "Dim testing as String = \"Testing\"",
                "Dim testing2 as String = \"Testing2\""
            };
            var nodeBody = _subNode.Statements;
            foreach (var e in expressions)
            {
                nodeBody = nodeBody.Add(SyntaxFactory.ParseExecutableStatement(e));
            }
            _subNode = _subNode.WithStatements(nodeBody);

            var commentMethodFunc = _methodBlockActions.GetCommentMethodAction("this is a comment");
            var newNode = commentMethodFunc(_syntaxGenerator, _subNode);

            foreach (var e in expressions)
            {
                StringAssert.Contains($"' {e}", newNode.ToFullString());
            }
        }

        [Test]
        public void AddExpressionToMethod()
        {
            var nodeBody = _subNode.Statements;
            nodeBody = nodeBody.Add(SyntaxFactory.ParseExecutableStatement("Dim testing as String = \"Testing\""));
            _subNode = _subNode.WithStatements(nodeBody);

            const string expression = "Dim i as Integer = 5";
            var addExpressionFunction = _methodBlockActions.GetAddExpressionToMethodAction(expression);
            var newNode = addExpressionFunction(_syntaxGenerator, _subNode);

            StringAssert.Contains(expression, newNode.ToFullString());
        }

        [Test]
        public void AddExpressionToFunction()
        {
            const string expression = "Await Task.Delay(1000)";
            var addExpressionFunc = _methodBlockActions.GetAddExpressionToMethodAction(expression);
            var newNode = addExpressionFunc(_syntaxGenerator, _functionNode);
            
            StringAssert.Contains(expression, newNode.ToFullString());
            Assert.IsTrue(newNode.Statements.Last().IsKind(SyntaxKind.ReturnStatement));
        }

        [Test]
        public void AddParametersToMethod()
        {
            var addParamsAction = _methodBlockActions.GetAddParametersToMethodAction("String,String", "param1,param2");
            var newNode = addParamsAction(_syntaxGenerator, _subNode);
            StringAssert.Contains("param1 As String", newNode.ToFullString());
        }

        [Test]
        public void ChangeMethodToReturnTask()
        {
            var changeReturnToTaskFunc = _methodBlockActions.GetChangeMethodToReturnTaskTypeAction();
            var newNode = changeReturnToTaskFunc(_syntaxGenerator, _subNode);
            
            StringAssert.Contains("Task", newNode.ToFullString());
            StringAssert.Contains("Async", newNode.ToFullString());
            Assert.True(newNode.SubOrFunctionStatement.IsKind(SyntaxKind.FunctionStatement));

            var newFunction = changeReturnToTaskFunc(_syntaxGenerator, _functionNode);
            StringAssert.Contains("Async", newFunction.ToFullString());
            StringAssert.Contains("Task(Of Integer)", newFunction.ToFullString());
        }

        [Test]
        public void ChangeMethodName()
        {
            var changeMethodNameFunc = _methodBlockActions.GetChangeMethodNameAction("NewName");

            var newNode = changeMethodNameFunc(_syntaxGenerator, _subNode);
            var newFunction = changeMethodNameFunc(_syntaxGenerator, _functionNode);
            
            StringAssert.Contains("Sub NewName", newNode.ToFullString());
            StringAssert.Contains("Function NewName", newFunction.ToFullString());
        }
    }
}