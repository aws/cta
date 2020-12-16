using System;
using CTA.Rules.Actions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class InvocationExpressionActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private InvocationExpressionActions _invocationExpressionActions;
        private InvocationExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _invocationExpressionActions = new InvocationExpressionActions();
            _node = SyntaxFactory.ParseExpression("Math.Abs(-1)") as InvocationExpressionSyntax;
        }

        [Test]
        public void GetAppendMethodAction_Appends_A_Method_Invocation()
        {
            const string invocationToAppend = "ToString()";
            var appendMethodFunc =
                _invocationExpressionActions.GetAppendMethodAction(invocationToAppend);
            var newNode = appendMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "Math.Abs(-1).ToString()";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }
    }
}