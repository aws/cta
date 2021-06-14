
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class IdentifierNameAction : GenericAction
    {
        public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> IdentifierNameActionFunc { get; set; }

        public new IdentifierNameAction Clone() => (IdentifierNameAction)this.MemberwiseClone();
        public override bool Equals(object obj)
        {
            var action = (IdentifierNameAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.IdentifierNameActionFunc.Method.Name == this.IdentifierNameActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (IdentifierNameActionFunc != null ? IdentifierNameActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}
