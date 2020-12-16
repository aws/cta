using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Linq;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on Compilation Units
    /// </summary>
    public class CompilationUnitActions
    {
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetAddDirectiveAction(string @namespace)
        {
            Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> AddDirective = (SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node) =>
            {
                var allUsings = node.Usings;

                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace)).NormalizeWhitespace();
                allUsings = allUsings.Add(usingDirective);

                node = node.WithUsings(allUsings).NormalizeWhitespace();
                return node;
            };
            return AddDirective;
        }

        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetRemoveDirectiveAction(string @namespace)
        {
            Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> RemoveDirective = (SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node) =>
            {
                var allUsings = node.Usings;
                var removeList = allUsings.Where(u => @namespace == u.Name.ToString());

                foreach (var item in removeList)
                {
                    allUsings = allUsings.Remove(item);
                }
                node = node.WithUsings(allUsings).NormalizeWhitespace();
                return node;
            };
            return RemoveDirective;
        }
    }
}
