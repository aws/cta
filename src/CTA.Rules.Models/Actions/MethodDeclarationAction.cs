
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;

namespace CTA.Rules.Models
{
    public class MethodDeclarationAction : GenericAction
    {
        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> MethodDeclarationActionFunc { get; set; }
        public override bool Equals(object obj)
        {
            var action = (MethodDeclarationAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.MethodDeclarationActionFunc.Method.Name == this.MethodDeclarationActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (MethodDeclarationActionFunc != null ? MethodDeclarationActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}