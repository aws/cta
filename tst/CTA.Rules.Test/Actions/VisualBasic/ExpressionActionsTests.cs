using CTA.Rules.Actions.VisualBasic;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System;

namespace CTA.Rules.Test.Actions.VisualBasic
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
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _expressionActions = new ExpressionActions();
            _node = SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("Math.Abs(-1)")
                .WithLeadingTrivia(SyntaxFactory.CommentTrivia($"' Existing Comment{Environment.NewLine}")));
        }

        [Test]
        public void GetAddAwaitOperatorAction()
        {
            var addAwaitFunc =
                _expressionActions.GetAddAwaitOperatorAction("");
            var newNode = addAwaitFunc(_syntaxGenerator, _node);

            var expectedResult = "' Existing Comment\r\nAwait Math.Abs(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }


        [Test]
        public void ExpressionActionAddComment()
        {
            var comment = "Super comment";
            var expressionAction = SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("var t = 1+5"));

            var addCommentFunc = _expressionActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, expressionAction);

            var expectedResult = @"' Added by CTA: Super comment
var t = 1+5";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void ObjectCreationAddComment()
        {
            var comment = "Super comment";
            var objectNode = _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("StringBuilder"))
                    .NormalizeWhitespace() as ObjectCreationExpressionSyntax;

            objectNode =
                objectNode.AddArgumentListArguments(
                    SyntaxFactory.SimpleArgument(SyntaxFactory.ParseExpression(@"""SomeText""")));
            
            var addCommentFunc = _expressionActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, objectNode);

            var expectedResult = @"' Added by CTA: Super comment
New StringBuilder(""SomeText"")";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void InvocationExpressionAddComment()
        {
            var comment = "Super comment";

            var addCommentFunc = _expressionActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @"' Existing Comment
' Added by CTA: Super comment
Math.Abs(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }
    }
}