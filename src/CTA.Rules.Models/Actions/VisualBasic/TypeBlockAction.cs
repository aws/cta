using System;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CTA.Rules.Models.VisualBasic
{
    public class TypeBlockAction : GenericAction
    {
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> TypeBlockActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (TypeBlockAction)obj;
            return action?.Key == this.Key
                   && action?.Value == this.Value
                   && action?.TypeBlockActionFunc.Method.Name == this.TypeBlockActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, TypeBlockActionFunc?.Method.Name);
        }
    }
}
