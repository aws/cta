using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Extensions
{
    public static class SyntaxTreeExtensions
    {
        public static IEnumerable<TypeDeclarationSyntax> GetNamespaceLevelTypes(this SyntaxTree tree)
        {
            return tree.GetRoot().DescendantNodes(node => !(node is TypeDeclarationSyntax)).OfType<TypeDeclarationSyntax>();
        }
    }
}
