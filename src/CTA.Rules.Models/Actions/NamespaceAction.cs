
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class NamespaceAction : GenericAction
    {
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> NamespaceActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (NamespaceAction)obj;
            return action?.Value == this.Value
                && action?.NamespaceActionFunc.Method.Name == this.NamespaceActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(3 * Value?.GetHashCode() ?? 0,
                5 * NamespaceActionFunc?.Method.Name.GetHashCode() ?? 0);
        }
    }
}
