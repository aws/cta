using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class ExpressionAction : GenericAction
    {
        public Func<SyntaxGenerator, ExpressionSyntax, ExpressionSyntax> ExpressionActionFunc { get; set; }
        public override bool Equals(object obj)
        {
            var action = (ExpressionAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.ExpressionActionFunc.Method.Name == this.ExpressionActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (ExpressionActionFunc != null ? ExpressionActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}
