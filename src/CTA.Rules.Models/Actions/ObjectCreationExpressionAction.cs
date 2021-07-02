
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class ObjectCreationExpressionAction : GenericAction
    {
        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> ObjectCreationExpressionGenericActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (ObjectCreationExpressionAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.ObjectCreationExpressionGenericActionFunc.Method.Name == this.ObjectCreationExpressionGenericActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return 3 * Value.GetHashCode()
                + 5 * (ObjectCreationExpressionGenericActionFunc != null ? ObjectCreationExpressionGenericActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}
