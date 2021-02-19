using CTA.Rules.Models;
using CTA.Rules.Update;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class MemberAccessActionsTests
    {
        private MemberAccessActions _memberAccessActions;
        private SyntaxGenerator _syntaxGenerator;
        private MemberAccessExpressionSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _memberAccessActions = new MemberAccessActions();
            _node = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName("value"));
        }

        [Test]
        public void MemberAccessAddComment()
        {
            const string comment = "This is a comment";
            var addCommentFunc = _memberAccessActions.GetAddCommentAction(comment);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            StringAssert.Contains(comment, newNode.ToFullString());
        }

        [Test]
        public void MemberAccessActionComparison()
        {
            var memberAccessAction = new MemberAccessAction()
            {
                Key = "Test",
                Value = "Test2",
                MemberAccessActionFunc = _memberAccessActions.GetAddCommentAction("NewAttribute")
            };

            var cloned = memberAccessAction.Clone();

            Assert.True(memberAccessAction.Equals(cloned));
            cloned.Value = "DifferentValue";
            Assert.False(memberAccessAction.Equals(cloned));
        }
    }
}