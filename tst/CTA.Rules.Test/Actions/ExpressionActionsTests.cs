using CTA.Rules.Actions.Csharp;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class ExpressionActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private ExpressionActions _expressionActions;
        private ExpressionStatementSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _expressionActions = new ExpressionActions();
            _node = SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("/* Super comment */ Math.Abs(-1)"));
        }

        [Test]
        public void GetAddAwaitOperatorAction()
        {
            var addAwaitFunc =
                _expressionActions.GetAddAwaitOperatorAction("");
            var newNode = addAwaitFunc(_syntaxGenerator, _node);

            var expectedResult = "/* Super comment */\r\nawait Math.Abs(-1);";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }


        [Test]
        public void ExpressionActionAddComment()
        {
            var comment = "Super comment";
            var expressionAction = SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("var t = 1+5"));

            var addCommentFunc = _expressionActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, expressionAction);

            var expectedResult = @"/* Added by CTA: Super comment */
var t  =  1 + 5 ;";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void ObjectCreationAddComment()
        {
            var comment = "Super comment";
            var objectnode = _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("StringBuilder"))
                    .NormalizeWhitespace() as ObjectCreationExpressionSyntax;

            objectnode = objectnode.AddArgumentListArguments(SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(
                        SyntaxFactory.TriviaList(),
                        "\"SomeText\"",
                        "\"SomeText\"",
                        SyntaxFactory.TriviaList()))));

            var addCommentFunc = _expressionActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, objectnode);

            var expectedResult = @"/* Added by CTA: Super comment */
new StringBuilder(""SomeText"")";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void InvocationExpressionAddComment()
        {
            var comment = "Super comment";
            var invocationNode = SyntaxFactory.ParseExpression("/* Comment */ Math.Abs(-1)") as InvocationExpressionSyntax;

            var addCommentFunc = _expressionActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, invocationNode);

            var expectedResult = @"/* Comment */
/* Added by CTA: Super comment */
Math.Abs(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }
    }
}