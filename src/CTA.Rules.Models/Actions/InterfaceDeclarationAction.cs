using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class InterfaceDeclarationAction : GenericAction
    {
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> InterfaceDeclarationActionFunc { get; set; }

        public new InterfaceDeclarationAction Clone() => (InterfaceDeclarationAction)this.MemberwiseClone();
        public override bool Equals(object obj)
        {
            var action = (InterfaceDeclarationAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.InterfaceDeclarationActionFunc.Method.Name == this.InterfaceDeclarationActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (InterfaceDeclarationActionFunc != null ? InterfaceDeclarationActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}
