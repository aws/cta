using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CTA.Rules.Actions.VisualBasic
{
    /// <summary>
    /// List of actions that can run on Compilation Units
    /// </summary>
    public class CompilationUnitActions
    {
        public Func<SyntaxGenerator, CompilationUnitSyntax,CompilationUnitSyntax> GetAddStatementAction(string @namespace)
        {
            CompilationUnitSyntax AddStatement(SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node)
            {
                var allImports = node.Imports;
                var importStatement = SyntaxFactory.ImportsStatement(
                        SyntaxFactory.SeparatedList<ImportsClauseSyntax>()
                            .Add(SyntaxFactory.SimpleImportsClause(
                                SyntaxFactory.ParseName(@namespace)))
                    )
                    .NormalizeWhitespace();
                allImports = allImports.Add(importStatement);

                node = node.WithImports(allImports).NormalizeWhitespace();
                return node;
            }
            return AddStatement;
        }

        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetRemoveStatementAction(string @namespace)
        {
            CompilationUnitSyntax RemoveStatement(SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node)
            {
                // remove duplicate directive references, don't use List based approach because
                // since we will be replacing the node after each loop, it update text span which will not remove duplicate namespaces
                var allImports = node.Imports;

                // difference in visual basic is that a single import statement can have multiple import clauses (namespaces)
                var removeItem = allImports.FirstOrDefault(i => i.ImportsClauses.Any(c => c.ToString() == @namespace));
                if (removeItem == null)
                {
                    return node;
                }
                allImports = allImports.Remove(removeItem);
                // re-add import if it had multiple clauses
                if (removeItem.ImportsClauses.Count > 1)
                {
                    var removeClause = removeItem.ImportsClauses.FirstOrDefault(c => c.ToString() == @namespace);
                    if (removeClause != null)
                    {
                        var newClauses = removeItem.ImportsClauses.Remove(removeClause);
                        var newImportStatement = removeItem.WithImportsClauses(newClauses);
                        allImports = allImports.Add(newImportStatement);
                    }
                }
                node = node.WithImports(allImports).NormalizeWhitespace();
                return RemoveStatement(syntaxGenerator, node);
            }

            return RemoveStatement;
        }

        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetAddCommentAction(string comment)
        {
            CompilationUnitSyntax AddComment(SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                //TODO see if this will lead NPE    
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, string.Format(Constants.VbCommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
    }
}
