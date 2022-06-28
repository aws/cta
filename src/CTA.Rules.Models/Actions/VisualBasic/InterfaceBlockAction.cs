using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models.Actions.VisualBasic
{
    public class InterfaceBlockAction : GenericAction
    {
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> InterfaceBlockActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (InterfaceBlockAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.InterfaceBlockActionFunc.Method.Name == this.InterfaceBlockActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, InterfaceBlockActionFunc?.Method.Name);
        }
    }
}
