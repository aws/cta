using CTA.Rules.Actions.VisualBasic;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class VisualBasicInvocationExpressionActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private InvocationExpressionActions _invocationExpressionActions;
        private InvocationExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _invocationExpressionActions = new InvocationExpressionActions();
            _node = SyntaxFactory.ParseExpression("' Comment \nMath.Abs(-1)") as InvocationExpressionSyntax;
        }

        [Test]
        public void GetReplaceMethodWithObjectAndParametersAction()
        {
            const string newMethod = "Math.Floor";
            const string newParameter = "(-2)";
            var replaceMethodFunc =
                _invocationExpressionActions.GetReplaceMethodWithObjectAndParametersAction(newMethod, newParameter);
            var newNode = replaceMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nMath.Floor(-2)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodWithObjectAction()
        {
            const string newMethod = "Math.Floor";
            var replaceMethodFunc =
                _invocationExpressionActions.GetReplaceMethodWithObjectAction(newMethod);
            var newNode = replaceMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nMath.Floor(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodWithObjectAddTypeAction()
        {
            _node = SyntaxFactory.ParseExpression("' Comment \r\nDependencyResolver.Current.GetService(Of Object)()") as InvocationExpressionSyntax;
            const string newMethod = "DependencyResolver.Current.GetService";
            var replaceMethodFunc =
                _invocationExpressionActions.GetReplaceMethodWithObjectAddTypeAction(newMethod);
            var newNode = replaceMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nDependencyResolver.Current.GetService(TypeOf Object)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodAndParametersAction()
        {
            const string newMethod = "Floor";
            const string newParameter = "(-2)";
            var replaceMethodFunc =
                _invocationExpressionActions.GetReplaceMethodAndParametersAction("Abs", newMethod, newParameter);
            var newNode = replaceMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nMath.Floor(-2)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceMethodOnlyAction()
        {
            const string newMethod = "Floor";
            var replaceMethodFunc =
                _invocationExpressionActions.GetReplaceMethodOnlyAction("Abs",newMethod);
            var newNode = replaceMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nMath.Floor(-1)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceParameterOnlyAction()
        {
            const string newParam = "(8)";
            var replaceMethodFunc =
                _invocationExpressionActions.GetReplaceParametersOnlyAction(newParam);
            var newNode = replaceMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nMath.Abs(8)";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetAppendMethodAction_Appends_A_Method_Invocation()
        {
            const string invocationToAppend = "ToString()";
            var appendMethodFunc =
                _invocationExpressionActions.GetAppendMethodAction(invocationToAppend);
            var newNode = appendMethodFunc(_syntaxGenerator, _node);

            var expectedResult = "' Comment \r\nMath.Abs(-1).ToString()";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }


        [Test]
        public void InvocationExpressionEquals()
        {
            var invocationExpressionAction = new InvocationExpressionAction<InvocationExpressionSyntax>() { Key = "Test", Value = "Test2", InvocationExpressionActionFunc = _invocationExpressionActions.GetAddCommentAction("Test") };
            var cloned = invocationExpressionAction.Clone<InvocationExpressionAction<InvocationExpressionSyntax>>();
            Assert.True(invocationExpressionAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(invocationExpressionAction.Equals(cloned));
        }
    }
}