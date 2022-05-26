using CTA.Rules.Models;
using CTA.Rules.Update;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using SimpleNameSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.SimpleNameSyntax;
using VbMemberAccessExpressionSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax;
using VbSyntaxFactory = Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace CTA.Rules.Test.Actions
{
    public class MemberAccessActionsTests
    {
        private MemberAccessActions _memberAccessActions;
        private SyntaxGenerator _syntaxGenerator;
        private MemberAccessExpressionSyntax _node;

        private SyntaxGenerator _vbGenerator;
        private VbMemberAccessExpressionSyntax _vbNode;
        
        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _memberAccessActions = new MemberAccessActions();
            _node = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName("value"));
            _vbGenerator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.VisualBasic);
            _vbNode = VbSyntaxFactory.MemberAccessExpression(
                Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.SimpleMemberAccessExpression,
                VbSyntaxFactory.ParseExpression("Math.Abs(-1)"),
                VbSyntaxFactory.Token(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.DotToken),
                (SimpleNameSyntax)VbSyntaxFactory.ParseName("Value"));
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

            var cloned = memberAccessAction.Clone<MemberAccessAction>();

            Assert.True(memberAccessAction.Equals(cloned));
            cloned.Value = "DifferentValue";
            Assert.False(memberAccessAction.Equals(cloned));
        }

        [Test]
        public void RemoveMemberAccess()
        {
            var removeMemberAccessFunc = _memberAccessActions.GetRemoveMemberAccessAction("");
            var newCsharpNode = removeMemberAccessFunc(_syntaxGenerator, _node);
            var newVbNode = removeMemberAccessFunc(_vbGenerator, _vbNode);

            Assert.AreEqual("this", newCsharpNode.ToFullString());
            Assert.AreEqual("Math.Abs(-1)", newVbNode.ToFullString());
        }
    }
}