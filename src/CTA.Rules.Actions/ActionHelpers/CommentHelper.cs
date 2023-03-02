using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using VB = Microsoft.CodeAnalysis.VisualBasic;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using System;

namespace CTA.Rules.Actions.ActionHelpers
{
    public static class CommentHelper
    {
        public static SyntaxNode AddCSharpComment(SyntaxNode node, string comment, string dontUseCTAPrefix = null)
        {
            if (node == null)
            {
                return node;
            }

            SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();

            var commentFormat = dontUseCTAPrefix != null ? Constants.CommentFormatBlank : Constants.CommentFormat;
            leadingTrivia = leadingTrivia.Add(CSharp.SyntaxFactory.SyntaxTrivia(CSharp.SyntaxKind.MultiLineCommentTrivia, string.Format(commentFormat, comment) + Environment.NewLine));
            node = node.WithLeadingTrivia(leadingTrivia);
            return node;
        }

        public static SyntaxNode AddVBComment(SyntaxNode node, string comment, string dontUseCTAPrefix = null)
        {
            if (node == null)
            {
                return node;
            }

            SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();

            var commentFormat = !string.IsNullOrEmpty(dontUseCTAPrefix) ? Constants.VbCommentFormatBlank : Constants.VbCommentFormat;
            leadingTrivia = leadingTrivia.Add(VB.SyntaxFactory.SyntaxTrivia(VB.SyntaxKind.CommentTrivia, string.Format(commentFormat, comment) + Environment.NewLine));
            node = node.WithLeadingTrivia(leadingTrivia);
            return node;            
        }
    }
}
