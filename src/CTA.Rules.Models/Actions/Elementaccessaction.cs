
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using Codelyzer.Analysis.Model;

namespace CTA.Rules.Models
{
    public class ElementAccessAction : GenericAction
    {
        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> InvocationExpressionActionFunc { get; set; }
        public override bool Equals(object obj)
        {
            var action = (ElementAccessAction)obj;
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