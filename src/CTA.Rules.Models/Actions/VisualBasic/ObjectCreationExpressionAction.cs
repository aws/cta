using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models.Actions.VisualBasic
{
    public class ObjectCreationExpressionAction : GenericAction
    {
        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> ObjectCreationExpressionGenericActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (ObjectCreationExpressionAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.ObjectCreationExpressionGenericActionFunc.Method.Name == this.ObjectCreationExpressionGenericActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, ObjectCreationExpressionGenericActionFunc?.Method.Name);
        }
    }
}
