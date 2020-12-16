
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using Codelyzer.Analysis.Model;
using System.Collections.Generic;

namespace CTA.Rules.Models
{
    public class ClassDeclarationAction : GenericAction
    {
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> ClassDeclarationActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (ClassDeclarationAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.ClassDeclarationActionFunc.Method.Name == this.ClassDeclarationActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (ClassDeclarationActionFunc != null ? ClassDeclarationActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}