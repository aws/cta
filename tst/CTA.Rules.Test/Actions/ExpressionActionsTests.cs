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
        public void InvocationExpressionEquals()
        {
            var expressionAction = new ExpressionAction() { Key = "Test", Value = "Test2", ExpressionActionFunc = _expressionActions.GetAddCommentAction("Test") };
            var cloned = expressionAction.Clone<ExpressionAction>();
            Assert.True(expressionAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(expressionAction.Equals(cloned));
        }
    }
}