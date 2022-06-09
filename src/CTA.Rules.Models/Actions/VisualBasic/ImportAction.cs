using System;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CTA.Rules.Models.Actions.VisualBasic { 

    public class ImportAction : GenericAction
    {
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> ImportActionFunc { get; set; }
        public Func<SyntaxGenerator, SimpleImportsClauseSyntax, SimpleImportsClauseSyntax> ImportsClauseActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (UsingAction)obj;
            return action?.Value == Value &&
                   (action?.UsingActionFunc.Method.Name == ImportActionFunc.Method.Name ||
                    action?.NamespaceUsingActionFunc.Method.Name == ImportsClauseActionFunc.Method.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, ImportActionFunc?.Method.Name);
        }
    } 
}
