using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTA.WebForms2Blazor.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Extensions
{
    public static class CommentingExtensions
    {
        public static SyntaxType AddComment<SyntaxType>(
            this SyntaxType node,
            IEnumerable<string> commentText,
            bool isLeading = true,
            int? lineCharacterSoftLimit = null)
            where SyntaxType : SyntaxNode
        {
            var lines = commentText;

            if (lineCharacterSoftLimit != null)
            {
                // Re-wrap words if limit is specified, this servers to maintain separation
                // between comment statements while still wrapping them
                lines = commentText.SelectMany(line => CommentWordWrap(line, (int)lineCharacterSoftLimit));
            }

            var triviaList = GetCommentLinesAsTriviaList(lines);

            return isLeading ?
                node.WithLeadingTrivia(node.GetLeadingTrivia().Concat(triviaList)) :
                node.WithTrailingTrivia(node.GetTrailingTrivia().Concat(triviaList));
        }

        public static SyntaxType AddComment<SyntaxType>(
            this SyntaxType node,
            string commentText,
            int lineCharacterSoftLimit = Constants.DefaultCommentLineCharacterLimit,
            bool isLeading = true)
            where SyntaxType : SyntaxNode
        {
            return node.AddComment(CommentWordWrap(commentText, lineCharacterSoftLimit), isLeading);
        }

        public static IEnumerable<SyntaxType> AddComment<SyntaxType>(
            this IEnumerable<SyntaxType> nodeSet,
            IEnumerable<string> commentText,
            bool isLeading = true,
            int? lineCharacterSoftLimit = null)
            where SyntaxType : SyntaxNode
        {
            var nodeToModify = isLeading ? nodeSet.FirstOrDefault() : nodeSet.LastOrDefault();

            if (nodeToModify == null)
            {
                return nodeSet;
            }

            nodeToModify = nodeToModify.AddComment(commentText, isLeading, lineCharacterSoftLimit);
            // We don't have to worry about 1 element skips because we've checked that at least
            // one node exists in the collection
            return isLeading ? nodeSet.Skip(1).Prepend(nodeToModify) : nodeSet.SkipLast(1).Append(nodeToModify);
        }

        public static IEnumerable<SyntaxType> AddComment<SyntaxType>(
            this IEnumerable<SyntaxType> nodeSet,
            string commentText,
            int lineCharacterSoftLimit = Constants.DefaultCommentLineCharacterLimit,
            bool isLeading = true)
            where SyntaxType : SyntaxNode
        {
            return nodeSet.AddComment(CommentWordWrap(commentText, lineCharacterSoftLimit), isLeading);
        }

        public static ClassDeclarationSyntax AddClassBlockComment(
            this ClassDeclarationSyntax classDeclaration,
            IEnumerable<string> commentText,
            bool atStart = true,
            int? lineCharacterSoftLimit = null)
        {
            var lines = commentText;

            if (lineCharacterSoftLimit != null)
            {
                // Re-wrap words if limit is specified, this servers to maintain separation
                // between comment statements while still wrapping them
                lines = commentText.SelectMany(line => CommentWordWrap(line, (int)lineCharacterSoftLimit));
            }
            
            // Lines attached to braces require an extra tab
            var triviaList = GetCommentLinesAsTriviaList(lines, 1);

            if (atStart)
            {
                return classDeclaration.WithOpenBraceToken(SyntaxFactory.Token(
                    classDeclaration.OpenBraceToken.LeadingTrivia,
                    SyntaxKind.OpenBraceToken,
                    // For start, place after open brace
                    SyntaxFactory.TriviaList(classDeclaration.OpenBraceToken.TrailingTrivia.Concat(triviaList))));
            }
            else
            {
                return classDeclaration.WithCloseBraceToken(SyntaxFactory.Token(
                    // For end, place before close brace
                    SyntaxFactory.TriviaList(classDeclaration.OpenBraceToken.LeadingTrivia.Concat(triviaList)),
                    SyntaxKind.CloseBraceToken,
                    classDeclaration.OpenBraceToken.TrailingTrivia));
            }
        }

        public static ClassDeclarationSyntax AddClassBlockComment(
            this ClassDeclarationSyntax classDeclaration,
            string commentText,
            int lineCharacterSoftLimit = Constants.DefaultCommentLineCharacterLimit,
            bool atStart = true)
        {
            return classDeclaration.AddClassBlockComment(CommentWordWrap(commentText, lineCharacterSoftLimit), atStart);
        }

        private static IEnumerable<string> CommentWordWrap(string commentText, int lineCharacterSoftLimit)
        {
            var lines = new List<string>();
            var words = commentText.Split(" ");
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word))
                {
                    continue;
                }

                // Use greater than because right now there's an extra
                // space at the end, though the exact line length isn't
                // really a big deal anyway
                if (currentLine.Length > lineCharacterSoftLimit)
                {
                    // Cut off extra space at the end
                    lines.Add(currentLine.ToString().Trim());
                    currentLine = new StringBuilder();
                }

                currentLine.Append($"{word} ");
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            return lines;
        }

        private static SyntaxTriviaList GetCommentLinesAsTriviaList(IEnumerable<string> commentText, int extraTabs = 0)
        {
            return SyntaxFactory.TriviaList(commentText.Select(commentLine =>
                {
                    var sb = new StringBuilder();

                    for (int i = 0; i < extraTabs; i++)
                    {
                        // For some reason roslyn insists on
                        // only using spaces so we use normal
                        // spaces to keep form
                        sb.Insert(sb.Length, " ", Constants.SpacesPerCommentTab);
                    }

                    sb.Append(Constants.DefaultCommentStartToken + commentLine);
                    return SyntaxFactory.Comment(sb.ToString());
                }));
        }
    }
}
