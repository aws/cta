
using System;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class IdentifierNameAction<T> : GenericAction
    {
        public Func<SyntaxGenerator, T, T> IdentifierNameActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (IdentifierNameAction<T>)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.IdentifierNameActionFunc.Method.Name == this.IdentifierNameActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, IdentifierNameActionFunc?.Method.Name);
        }

        public IdentifierNameAction<T> Copy()
        {
            IdentifierNameAction<T> copy = (IdentifierNameAction<T>)base.Copy();
            copy.IdentifierNameActionFunc = this.IdentifierNameActionFunc;
            return copy;
        }
    }
}
