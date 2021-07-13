
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class ClassDeclarationAction : GenericAction
    {
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> ClassDeclarationActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (ClassDeclarationAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action?.ClassDeclarationActionFunc.Method.Name == this.ClassDeclarationActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, ClassDeclarationActionFunc?.Method.Name);
        }
    }
}
