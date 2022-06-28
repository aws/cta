using CTA.Rules.Actions.Csharp;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class MethodDeclarationActionsTests
    {
        private MethodDeclarationActions _methodDeclarationActions;
        private SyntaxGenerator _syntaxGenerator;
        private MethodDeclarationSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _methodDeclarationActions = new MethodDeclarationActions();

            _node = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Authorize")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(@"/*Nothing to see here*/")));
        }

        [Test]
        public void MethodDeclarationAddComment()
        {
            const string comment = "This is a comment";
            var addCommentFunction = _methodDeclarationActions.GetAddCommentAction(comment);
            var newNode = addCommentFunction(_syntaxGenerator, _node);

            StringAssert.Contains(comment, newNode.ToFullString());
        }

        [Test]
        public void MethodDeclarationAddExpression()
        {
            BlockSyntax nodeBody = _node.Body;
            nodeBody = nodeBody.AddStatements(SyntaxFactory.ParseStatement("string testing = \"Testing\";"));
            _node = _node.WithBody(nodeBody);

            const string expression = "int i = 5;";
            var addExpressionFunction = _methodDeclarationActions.GetAppendExpressionAction(expression);
            var newNode = addExpressionFunction(_syntaxGenerator, _node);

            StringAssert.Contains(expression, newNode.ToFullString());
        }

        [Test]
        public void MethodDeclarationActionComparison()
        {
            var methodDeclarationAction = new MethodDeclarationAction()
            {
                Key = "Test",
                Value = "Test2",
                MethodDeclarationActionFunc = _methodDeclarationActions.GetAddCommentAction("NewAttribute")
            };

            var cloned = methodDeclarationAction.Clone<MethodDeclarationAction>();

            Assert.True(methodDeclarationAction.Equals(cloned));
            cloned.Value = "DifferentValue";
            Assert.False(methodDeclarationAction.Equals(cloned));
        }
    }
}