using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static SyntaxNode NoComments(this SyntaxNode node)
        {
            if (node != null)
            {
                var childNodes = node.ChildNodes();
                node = node.ReplaceNodes(childNodes, (node, returnedNode) => node.NoComments());
                node = node.WithoutTrivia();
            }
            return node;
        }
    }
}
