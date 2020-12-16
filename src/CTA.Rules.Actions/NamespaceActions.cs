using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTA.Rules.Actions
{
    public class NamespaceActions
    {
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> GetRenameNamespaceAction(string newName)
        {
            Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> RenameNamespace = (SyntaxGenerator syntaxGenerator, NamespaceDeclarationSyntax node) =>
            {
                node = node.WithName(SyntaxFactory.ParseName(newName));
                return node;
            };
            return RenameNamespace;
        }
    }
}
