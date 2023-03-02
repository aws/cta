using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.Csharp
{
    public class NamespaceActions
    {
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> GetRenameNamespaceAction(string newName)
        {
            NamespaceDeclarationSyntax RenameNamespace(SyntaxGenerator syntaxGenerator, NamespaceDeclarationSyntax node)
            {
                node = node.WithName(SyntaxFactory.ParseName(newName));
                return node;
            }
            return RenameNamespace;
        }

        /// <summary>
        /// Only support remove using directive actions inside Namespace block.
        /// The add using directive actions will be happening in CompiliationUnit.
        /// </summary>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> GetRemoveDirectiveAction(string @namespace)
        {
            NamespaceDeclarationSyntax RemoveDirective(SyntaxGenerator syntaxGenerator, NamespaceDeclarationSyntax node)
            {
                // remove duplicate directive references, don't use List based approach because
                // since we will be replacing the node after each loop, it update text span which will not remove duplicate namespaces
                var allUsings = node.Usings;
                var removeItem = allUsings.FirstOrDefault(u => @namespace == u.Name.ToString());

                if (removeItem == null)
                    return node;

                allUsings = allUsings.Remove(removeItem);

                node = node.WithUsings(allUsings);
                return RemoveDirective(syntaxGenerator, node);
            }
            return RemoveDirective;
        }
    }
}
