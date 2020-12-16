
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using Codelyzer.Analysis.Model;
using System.Collections.Generic;

namespace CTA.Rules.Models
{
    public class InvocationExpressionAction : GenericAction
    {
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> InvocationExpressionActionFunc { get; set; }
        public override bool Equals(object obj)
        {
            var action = (InvocationExpressionAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.InvocationExpressionActionFunc.Method.Name == this.InvocationExpressionActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (InvocationExpressionActionFunc != null ? InvocationExpressionActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}