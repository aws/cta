
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;

namespace CTA.Rules.Models
{
    public class UsingAction : GenericAction
    {
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> UsingActionFunc { get; set; }
        public override bool Equals(object obj)
        {
            var action = (UsingAction)obj;
            return action.Value == this.Value
                && action.UsingActionFunc.Method.Name == this.UsingActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return 3 * Value.GetHashCode()
                + 5 * (UsingActionFunc != null ? UsingActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}