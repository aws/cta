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

        [Test]
        public void GetReplaceObjectPropertyIdentifier()
        {
            string oldIdentifier = "FileSystem", newIdentifier = "FileProvider";
            _node = _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("FileServerOptions")).NormalizeWhitespace() as ObjectCreationExpressionSyntax;

            _node = _node.WithArgumentList(SyntaxFactory.ArgumentList()).WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(
                    new SyntaxNodeOrToken[]{
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("RequestPath"),
                            SyntaxFactory.ParseExpression("PathString.Empty")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("FileSystem"),
                            SyntaxFactory.ParseExpression("new PhysicalFileSystem(@\".\\defaults\")")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken)})))
            .NormalizeWhitespace();

            var replaceObjectWithInvocationFunc = _objectCreationExpressionActions.GetReplaceOrAddObjectPropertyIdentifierAction(oldIdentifier, newIdentifier, string.Empty);
            var newNode = replaceObjectWithInvocationFunc(_syntaxGenerator, _node);

            StringAssert.Contains(newIdentifier, newNode.ToFullString());
        }

        [Test]
        public void GetAddObjectPropertyIdentifier()
        {
            string oldIdentifier = "FileSystem", newIdentifier = "FileProvider", newValue = @"new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @""""))";
            _node = _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("FileServerOptions")).NormalizeWhitespace() as ObjectCreationExpressionSyntax;

            _node = _node.WithArgumentList(SyntaxFactory.ArgumentList()).WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(
                    new SyntaxNodeOrToken[]{
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("RequestPath"),
                            SyntaxFactory.ParseExpression("PathString.Empty")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("EnableDirectoryBrowsing"),
                            SyntaxFactory.ParseExpression("true")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken)})))
            .NormalizeWhitespace();

            var replaceObjectWithInvocationFunc = _objectCreationExpressionActions.GetReplaceOrAddObjectPropertyIdentifierAction(oldIdentifier, newIdentifier, newValue);
            var newNode = replaceObjectWithInvocationFunc(_syntaxGenerator, _node);

            StringAssert.Contains(newIdentifier, newNode.ToFullString());
            StringAssert.Contains(newValue, newNode.ToFullString());
        }

        [Test]
        public void GetReplaceObjectPropertyValue()
        {
            string oldIdentifier = "PhysicalFileSystem", newIdentifier = "PhysicalFileProvider";
            _node = _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("FileServerOptions")).NormalizeWhitespace() as ObjectCreationExpressionSyntax;

            _node = _node.WithArgumentList(SyntaxFactory.ArgumentList()).WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(
                    new SyntaxNodeOrToken[]{
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("RequestPath"),
                            SyntaxFactory.ParseExpression("PathString.Empty")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("FileSystem"),
                            SyntaxFactory.ParseExpression("new PhysicalFileSystem(@\".\\defaults\")")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken)})))
            .NormalizeWhitespace();

            var replaceObjectWithInvocationFunc = _objectCreationExpressionActions.GetReplaceObjectPropertyValueAction(oldIdentifier,newIdentifier);
            var newNode = replaceObjectWithInvocationFunc(_syntaxGenerator, _node);

            StringAssert.Contains(newIdentifier, newNode.ToFullString());
            StringAssert.DoesNotContain(oldIdentifier, newNode.ToFullString());
        }

        [Test]
        public void ObjectCreationExpressionEquals()
        {
            var objectCreationExpressionAction = new ObjectCreationExpressionAction() { Key = "Test", Value = "Test2", ObjectCreationExpressionGenericActionFunc = _objectCreationExpressionActions.GetAddCommentAction("Test") };
            var cloned = objectCreationExpressionAction.Clone();
            Assert.True(objectCreationExpressionAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(objectCreationExpressionAction.Equals(cloned));
        }
    }
}