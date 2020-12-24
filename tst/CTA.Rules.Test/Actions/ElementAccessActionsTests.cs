using CTA.Rules.Actions;
using CTA.Rules.Models;
using CTA.Rules.Update;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class ElementAccessActionsTests
    {
        private ElementAccessActions _elementAccessActions;
        private SyntaxGenerator _syntaxGenerator;
        private ElementAccessExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _elementAccessActions = new ElementAccessActions();
            _node = SyntaxFactory.ElementAccessExpression(SyntaxFactory.ParseExpression("Expression.ElementAccess"));
        }

        [Test]
        public void ElementAccessAddComment()
        {
            const string comment = "This is a comment";
            var addCommentFunc = _elementAccessActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            StringAssert.Contains(comment, newNode.ToFullString());
        }

        [Test]
        public void ElementAccessActionComparison()
        {
            var elementAccessAction = new ElementAccessAction()
            {
                Key = "Test",
                Value = "Test2",
                ElementAccessExpressionActionFunc = _elementAccessActions.GetAddCommentAction("Test")
                 
            };

            var cloned = elementAccessAction.Clone();

            Assert.True(elementAccessAction.Equals(cloned));
            cloned.Value = "DifferentValue";
            Assert.False(elementAccessAction.Equals(cloned));
        }
    }
}