using System;
using CTA.Rules.Actions.ActionHelpers;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Update.VisualBasic
{
    /// <summary>
    /// List of actions that can run on Element Accesses
    /// </summary>
    public class ElementAccessActions
    {
        public Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax> GetAddCommentAction(string comment)
        {
            MemberAccessExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, MemberAccessExpressionSyntax node)
            {
                return (MemberAccessExpressionSyntax)CommentHelper.AddVBComment(node, comment);
            }
            return AddComment;
        }

        public Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax> GetReplaceElementAccessAction(string newExpression)
        {
            MemberAccessExpressionSyntax ReplaceElement(SyntaxGenerator syntaxGenerator, MemberAccessExpressionSyntax node)
            {
                var addCommentFunc = GetAddCommentAction($"Replace with {newExpression}");
                return addCommentFunc(syntaxGenerator, node);
            }
            return ReplaceElement;
        }
    }
}
