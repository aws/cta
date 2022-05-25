using System;
using CTA.Rules.Models;
using CTA.Rules.Update.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class ElementAccessActionsTests
    {
        private ElementAccessActions _elementAccessActions;
        private SyntaxGenerator _syntaxGenerator;
        private MemberAccessExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _elementAccessActions = new ElementAccessActions();
            _node = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ParseExpression("Expression"), SyntaxFactory.Token(SyntaxKind.DotToken),
                (SimpleNameSyntax)SyntaxFactory.ParseName("ElementAccess"));
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
        public void ReplaceElementAccess()
        {
            const string expression = "ConfigurationManager.Configuration.GetSection(\"ConnectionStrings\")";
            var replaceElementAccessFunc = _elementAccessActions.GetReplaceElementAccessAction(expression);
            var newNode = replaceElementAccessFunc(_syntaxGenerator, _node);
            StringAssert.Contains($"' Added by CTA: Replace with {expression}", newNode.ToFullString());
        }

        [Test]
        public void ElementAccessActionComparison()
        {
            throw new NotImplementedException();
            // var elementAccessAction = new ElementAccessAction()
            // {
            //     Key = "Test",
            //     Value = "Test2",
            //     ElementAccessExpressionActionFunc = _elementAccessActions.GetAddCommentAction("Test")
            //
            // };
            //
            // var cloned = elementAccessAction.Clone<ElementAccessAction>();
            //
            // Assert.True(elementAccessAction.Equals(cloned));
            // cloned.Value = "DifferentValue";
            // Assert.False(elementAccessAction.Equals(cloned));
        }
    }
}