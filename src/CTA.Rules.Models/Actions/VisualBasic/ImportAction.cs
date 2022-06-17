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
            var action = (ImportAction)obj;
            return action?.Value == Value &&
                   (action?.ImportActionFunc.Method.Name == ImportActionFunc.Method.Name ||
                    action?.ImportsClauseActionFunc.Method.Name == ImportsClauseActionFunc.Method.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, ImportActionFunc?.Method.Name);
        }
    } 
}
