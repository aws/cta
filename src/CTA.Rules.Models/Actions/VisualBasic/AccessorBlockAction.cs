using System;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CTA.Rules.Models.Actions.VisualBasic
{
    public class AccessorBlockAction : GenericAction
    {
        public Func<SyntaxGenerator, AccessorBlockSyntax, AccessorBlockSyntax> AccessorBlockActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (AccessorBlockAction)obj;
            return action?.Key == this.Key
                   && action?.Value == this.Value
                   && action?.AccessorBlockActionFunc.Method.Name == this.AccessorBlockActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, AccessorBlockActionFunc?.Method.Name);
        }
    }
}
