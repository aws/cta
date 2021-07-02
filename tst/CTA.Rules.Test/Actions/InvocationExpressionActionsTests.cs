using CTA.Rules.Actions;
using CTA.Rules.Models;
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
        public void GetReplaceMethodWithObjectAndParametersAction()
        {
            const string newMethod = "Math.Floor";
            const string newParameter = "(-2)";
            var appendMethodFunc =
                _invocationExpressionActions.GetReplaceMethodWithObjectAndParametersAction(newMethod, newParameter);
            var newNode = appendMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "Math.Floor(-2)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodWithObjectAction()
        {
            const string newMethod = "Math.Floor";
            var appendMethodFunc =
                _invocationExpressionActions.GetReplaceMethodWithObjectAction(newMethod);
            var newNode = appendMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "Math.Floor(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodAndParametersAction()
        {
            const string newMethod = "Floor";
            const string newParameter = "(-2)";
            var appendMethodFunc =
                _invocationExpressionActions.GetReplaceMethodAndParametersAction("Abs", newMethod, newParameter);
            var newNode = appendMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "Math.Floor(-2)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodOnlyAction()
        {
            const string newMethod = "Floor";
            var appendMethodFunc =
                _invocationExpressionActions.GetReplaceMethodOnlyAction("Abs",newMethod);
            var newNode = appendMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "Math.Floor(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
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


        [Test]
        public void InvocationExpressionEquals()
        {
            var invocationExpressionAction = new InvocationExpressionAction() { Key = "Test", Value = "Test2", InvocationExpressionActionFunc = _invocationExpressionActions.GetAddCommentAction("Test") };
            var cloned = invocationExpressionAction.Clone<InvocationExpressionAction>();
            Assert.True(invocationExpressionAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(invocationExpressionAction.Equals(cloned));
        }
    }
}