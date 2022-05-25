using System;
using CTA.Rules.Actions.VisualBasic;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class AttributeListActionsTests
    {
        private AttributeListActions _attributeListActions;
        private SyntaxGenerator _syntaxGenerator;
        private AttributeListSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _attributeListActions = new AttributeListActions();
            var seperatedList = SyntaxFactory.SeparatedList<AttributeSyntax>();
            seperatedList.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName("Test")));
            _node = SyntaxFactory.AttributeList(seperatedList);
        }

        [Test]
        public void AttributeListAddComment()
        {
            const string comment = "This is a comment";
            var changeAttributeFunc = _attributeListActions.GetAddCommentAction(comment);
            var newNode = changeAttributeFunc(_syntaxGenerator, _node);

            StringAssert.Contains(comment, newNode.ToFullString());
        }

        [Test]
        public void AttributeListActionComparison()
        {
            throw new NotImplementedException();
            // var attributeAction = new AttributeAction()
            // {
            //     Key = "Test",
            //     Value = "Test2",
            //     AttributeListActionFunc = _attributeListActions.GetAddCommentAction("NewAttribute")
            // };
            //
            // var cloned = attributeAction.Clone<AttributeAction>();
            //
            // Assert.True(attributeAction.Equals(cloned));
            // cloned.Value = "DifferentValue";
            // Assert.False(attributeAction.Equals(cloned));
        }
    }
}