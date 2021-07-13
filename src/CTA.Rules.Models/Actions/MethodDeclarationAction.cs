
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class MethodDeclarationAction : GenericAction
    {
        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> MethodDeclarationActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (MethodDeclarationAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.MethodDeclarationActionFunc.Method.Name == this.MethodDeclarationActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, MethodDeclarationActionFunc?.Method.Name);
        }
    }
}
