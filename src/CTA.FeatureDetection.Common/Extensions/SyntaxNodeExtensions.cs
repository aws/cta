using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class SyntaxNodeExtensions
    {
        // A syntax tree has SyntaxNodes, SyntaxTokens and SyntaxTrivia, for more detail please refer to link below.
        // https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/syntax-analysis
        // This function is to remove all comments from the syntax tree, which it needs to iterate and replaces all
        // SyntaxNodes and SyntaxTokens to so no comments or white spaces are in the tree anymore.
        public static SyntaxNode NoComments(this SyntaxNode node)
        {
            if (node != null)
            {
                node = node.WithoutTrivia();

                var childNodes = node.ChildNodes();
                node = node.ReplaceNodes(childNodes, (oldNode, _) => oldNode.NoComments());

                var childTokens = node.ChildTokens();
                node = node.ReplaceTokens(childTokens, (oldToken, _) => oldToken.WithoutTrivia());
            }
            return node;
        }
    }
}
