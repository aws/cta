using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class ExpressionAction : GenericAction
    {
        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> ExpressionActionFunc { get; set; }
        public override bool Equals(object obj)
        {
            var action = (ExpressionAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.ExpressionActionFunc.Method.Name == this.ExpressionActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, ExpressionActionFunc?.Method.Name);
        }
    }
}
