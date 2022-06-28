
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class NamespaceAction<T> : GenericAction
    {
        public Func<SyntaxGenerator, T, T> NamespaceActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (NamespaceAction<T>)obj;
            return action?.Value == this.Value
                && action?.NamespaceActionFunc.Method.Name == this.NamespaceActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, NamespaceActionFunc?.Method.Name);
        }
    }
}
