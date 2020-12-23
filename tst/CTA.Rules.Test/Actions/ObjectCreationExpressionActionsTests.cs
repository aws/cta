using CTA.Rules.Actions;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class ObjectCreationExpressionActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private ObjectCreationExpressionActions _objectCreationExpressionActions;
        private ObjectCreationExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _objectCreationExpressionActions = new ObjectCreationExpressionActions();
            _node = _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("StringBuilder"))
                    .NormalizeWhitespace() as ObjectCreationExpressionSyntax;

            _node = _node.AddArgumentListArguments(SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(
                        SyntaxFactory.TriviaList(),
                        "\"SomeText\"",
                        "\"SomeText\"",
                        SyntaxFactory.TriviaList()))));
        }

        [Test]
        public void GetReplaceObjectWithInvocationAction_Replaces_Constructor_With_New_Invocation_And_Preserves_Args()
        {
            const string newStatement = "Console.WriteLine()";
            var replaceObjectWithInvocationFunc =
                _objectCreationExpressionActions.GetReplaceObjectWithInvocationAction(newStatement, "true");
            var newNode = replaceObjectWithInvocationFunc(_syntaxGenerator, _node);

            var expectedResult = "Console.WriteLine(\"SomeText\")";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }


        [Test]
        public void ObjectCreationExpressionActionEquals()
        {
            var objectCreationExpressionAction = new ObjectCreationExpressionAction() { Key = "Test", Value = "Test2", ObjectCreationExpressionGenericActionFunc = _objectCreationExpressionActions.GetReplaceObjectinitializationAction("Test") };
            var cloned = objectCreationExpressionAction.Clone();
            Assert.True(objectCreationExpressionAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(objectCreationExpressionAction.Equals(cloned));
        }
    }
}