using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models.Actions.VisualBasic
{
    public class MethodBlockAction : GenericAction
    {
        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> MethodBlockActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (MethodBlockAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.MethodBlockActionFunc.Method.Name == this.MethodBlockActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, MethodBlockActionFunc?.Method.Name);
        }
    }
}
