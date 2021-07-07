using CTA.Rules.Actions;
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
        private ExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _expressionActions = new ExpressionActions();
            _node = SyntaxFactory.ParseExpression("Math.Abs(-1)");
        }

        [Test]
        public void GetAddAwaitOperatorAction()
        {
            var addAwaitFunc =
                _expressionActions.GetAddAwaitOperatorAction("");
            var newNode = addAwaitFunc(_syntaxGenerator, _node);

            var expectedResult = "await Math.Abs(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }


        [Test]
        public void InvocationExpressionEquals()
        {
            var invocationExpressionAction = new ExpressionAction() { Key = "Test", Value = "Test2", ExpressionActionFunc = _expressionActions.GetAddCommentAction("Test") };
            var cloned = invocationExpressionAction.Clone();
            Assert.True(invocationExpressionAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(invocationExpressionAction.Equals(cloned));
        }
    }
}