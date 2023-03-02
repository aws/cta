using System;
using CTA.Rules.Actions.ActionHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Update
{
    /// <summary>
    /// List of actions that can run on Member Accesses
    /// </summary>
    public class MemberAccessActions
    {
        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetAddCommentAction(string comment)
        {
            SyntaxNode AddComment(SyntaxGenerator syntaxGenerator, SyntaxNode node)
            {
                if (node is Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax)
                {
                    return CommentHelper.AddVBComment(node, comment);
                }
                else
                {
                    return CommentHelper.AddCSharpComment(node, comment);
                }
            }
            return AddComment;
        }

        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetRemoveMemberAccessAction(string _)
        {
            static SyntaxNode RemoveMemberAccess(SyntaxGenerator syntaxGenerator, SyntaxNode node)
            {
                if(node is MemberAccessExpressionSyntax)
                {
                    return (node as MemberAccessExpressionSyntax).Expression;
                }
                if (node is Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax)
                {
                    return (node as Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax).Expression;
                }
                return node;
            }
            return RemoveMemberAccess;
        }
    }
}
