using System;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Linq;

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
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
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
                return node;
            }
            return RemoveMemberAccess;
        }
    }
}
