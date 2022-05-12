using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.VisualBasic
{
    public class NamespaceActions
    {
        public Func<SyntaxGenerator, NamespaceBlockSyntax, NamespaceBlockSyntax> GetRenameNamespaceAction(string newName)
        {
            NamespaceBlockSyntax RenameNamespace(SyntaxGenerator syntaxGenerator, NamespaceBlockSyntax node)
            {
                node = node.WithNamespaceStatement(SyntaxFactory.NamespaceStatement(SyntaxFactory.ParseName(newName)));
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
        public Func<SyntaxGenerator, NamespaceBlockSyntax, NamespaceBlockSyntax> GetRemoveDirectiveAction(string @namespace)
        {
            // Imports are not allowed within a namespace block in visual basic
            throw new NotImplementedException();
        }
    }
}
