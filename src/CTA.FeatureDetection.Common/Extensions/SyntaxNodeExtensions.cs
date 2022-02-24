using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class SyntaxNodeExtensions
    {
        // A syntax tree has SyntaxNodes, SyntaxTokens and SyntaxTrivia, for more detail please refer to link below:
        // https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/syntax-analysis
        // This function's purpose is to remove all trivia from the syntax tree, so it needs to iterate through all
        // existing SyntaxNodes and SyntaxTokens and to replace each of them without trivia.
        public static SyntaxNode RemoveAllTrivia(this SyntaxNode node)
        {
            if (node != null)
            {
                node = node.WithoutTrivia();

                var childNodes = node.ChildNodes();
                node = node.ReplaceNodes(childNodes, (oldNode, _) => oldNode.RemoveAllTrivia());

                var childTokens = node.ChildTokens();
                node = node.ReplaceTokens(childTokens, (oldToken, _) => oldToken.WithoutTrivia());
            }
            return node;
        }
    }
}
