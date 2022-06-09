
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class UsingAction : GenericAction
    {
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> UsingActionFunc { get; set; }
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> NamespaceUsingActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (UsingAction)obj;
            return action?.Value == this.Value
                    &&  (
                            (action?.UsingActionFunc.Method.Name == this.UsingActionFunc.Method.Name)
                            ||
                            (action?.NamespaceUsingActionFunc.Method.Name == this.NamespaceUsingActionFunc.Method.Name)
                        );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, UsingActionFunc?.Method.Name);
        }
    }
}
