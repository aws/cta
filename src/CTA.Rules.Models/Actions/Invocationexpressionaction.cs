
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class InvocationExpressionAction : GenericAction
    {
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> InvocationExpressionActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (InvocationExpressionAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.InvocationExpressionActionFunc.Method.Name == this.InvocationExpressionActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key?.GetHashCode() ?? 0,
                3 * Value?.GetHashCode() ?? 0,
                5 * InvocationExpressionActionFunc?.Method.Name.GetHashCode() ?? 0);
        }
    }
}
