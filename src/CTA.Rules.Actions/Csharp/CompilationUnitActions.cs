using System;
using System.Linq;
using CTA.Rules.Actions.ActionHelpers;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.Csharp
{
    /// <summary>
    /// List of actions that can run on Compilation Units
    /// </summary>
    public class CompilationUnitActions
    {
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetAddDirectiveAction(string @namespace)
        {
            CompilationUnitSyntax AddDirective(SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node)
            {
                var allUsings = node.Usings;

                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace))
                    .NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine + Environment.NewLine));

                allUsings = allUsings.Add(usingDirective);

                node = node.WithUsings(allUsings);
                return node;
            }
            return AddDirective;
        }

        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetRemoveDirectiveAction(string @namespace)
        {
            CompilationUnitSyntax RemoveDirective(SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node)
            {
                // remove duplicate directive references, don't use List based approach because
                // since we will be replacing the node after each loop, it update text span which will not remove duplicate namespaces
                var allUsings = node.Usings;
                var removeItem = allUsings.FirstOrDefault(u => @namespace == u.Name.ToString());

                if (removeItem == null)
                    return node;

                allUsings = allUsings.Remove(removeItem);
                
                node = node.WithUsings(allUsings);
                return RemoveDirective(syntaxGenerator,node);
            }
            return RemoveDirective;
        }

        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetAddCommentAction(string comment)
        {
            CompilationUnitSyntax AddComment(SyntaxGenerator syntaxGenerator, CompilationUnitSyntax node)
            {
                return (CompilationUnitSyntax)CommentHelper.AddCSharpComment(node, comment);
            }
            return AddComment;
        }
    }
}
