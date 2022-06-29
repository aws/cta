﻿using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class AttributeAction : GenericAction
    {
        public Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax> AttributeActionFunc { get; set; }
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> AttributeListActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (AttributeAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                &&
                (
                (action.AttributeActionFunc != null && this.AttributeActionFunc != null && action.AttributeActionFunc.Method.Name == this.AttributeActionFunc.Method.Name)
                ||
                (action.AttributeListActionFunc != null && this.AttributeListActionFunc != null && action.AttributeListActionFunc.Method.Name == this.AttributeListActionFunc.Method.Name)
                );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, AttributeActionFunc?.Method.Name, AttributeListActionFunc?.Method.Name);
        }
    }
}
