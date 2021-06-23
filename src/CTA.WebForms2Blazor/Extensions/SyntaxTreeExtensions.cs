using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace CTA.WebForms2Blazor.Extensions
{
    public static class SyntaxTreeExtensions
    {
        public const string DefaultCommentStartToken = "// ";
        // Arbitrary number, we can change this if need be
        public const int DefaultCommentLineCharacterLimit = 40;

        public static IEnumerable<TypeDeclarationSyntax> GetNamespaceLevelTypes(this SyntaxTree tree)
        {
            return tree.GetRoot().DescendantNodes(node => !(node is TypeDeclarationSyntax)).OfType<TypeDeclarationSyntax>();
        }

        public static SyntaxType AddComment<SyntaxType>(this SyntaxType node, IEnumerable<string> commentText) where SyntaxType : SyntaxNode
        {
            var triviaList = SyntaxFactory.TriviaList(commentText.Select(commentLine => SyntaxFactory.Comment(DefaultCommentStartToken + commentLine)));

            return node.WithLeadingTrivia(triviaList);
        }

        public static SyntaxType AddComment<SyntaxType>(this SyntaxType node, string commentText, int lineCharacterSoftLimit = DefaultCommentLineCharacterLimit)
            where SyntaxType : SyntaxNode
        {
            var lines = new List<string>();
            var words = commentText.Split(" ");
            var currentLine = string.Empty;

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
                    lines.Add(currentLine.Trim());
                    currentLine = string.Empty;
                }

                currentLine += $"{word} ";
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine.Trim());
            }

            return node.AddComment(lines);
        }

        public static IEnumerable<SyntaxType> UnionSyntaxNodeCollections<SyntaxType>(
            this IEnumerable<SyntaxType> nodeCollection1,
            IEnumerable<SyntaxType> nodeCollection2,
            bool overwriteCollection2Nodes = true) where SyntaxType : SyntaxNode
        {
            IEnumerable<SyntaxType> trimmedCollection;

            if (overwriteCollection2Nodes)
            {
                trimmedCollection = nodeCollection2.Where(node2 => nodeCollection1.All(node1 => !node1.IsEquivalentTo(node2, false)));
                return nodeCollection1.Concat(trimmedCollection);
            }

            trimmedCollection = nodeCollection1.Where(node1 => nodeCollection2.All(node2 => !node2.IsEquivalentTo(node1, false)));
            return trimmedCollection.Concat(nodeCollection1);
        }
    }
}
