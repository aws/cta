using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on attribute lists
    /// </summary>
    public class AttributeListActions
    {
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> GetAddCommentAction(string comment)
        {
            Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> AddComment = (SyntaxGenerator syntaxGenerator, AttributeListSyntax node) =>
            {
                //TODO IS there possibility of NPE , if there are no Trivia or it always returns a node...
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            };
            return AddComment;
        }
    }
}
