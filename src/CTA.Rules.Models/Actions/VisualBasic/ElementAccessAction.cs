using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models.VisualBasic
{
    public class ElementAccessAction : GenericAction
    {
        public Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax> ElementAccessExpressionActionFunc { get; set; }

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
