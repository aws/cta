using System;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Update
{
    /// <summary>
    /// List of actions that can run on Member Accesses
    /// </summary>
    public class MemberAccessActions
    {
        public Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax> GetAddCommentAction(string comment)
        {
            MemberAccessExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, MemberAccessExpressionSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
    }
}
