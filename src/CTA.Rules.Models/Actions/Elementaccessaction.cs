
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class ElementAccessAction : GenericAction
    {
        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> ElementAccessExpressionActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (ElementAccessAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.ElementAccessExpressionActionFunc.Method.Name == this.ElementAccessExpressionActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, ElementAccessExpressionActionFunc?.Method.Name);
        }
    }
}
