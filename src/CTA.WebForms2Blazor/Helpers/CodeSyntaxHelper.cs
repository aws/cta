using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Helpers
{
    public static class CodeSyntaxHelper
    {
        public static UsingDirectiveSyntax BuildUsingStatement(string referencedNamespace)
        {
            return SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(referencedNamespace));
        }

        public static IEnumerable<UsingDirectiveSyntax> BuildUsingStatements(IEnumerable<string> referencedNamespaces)
        {
            return referencedNamespaces.Select(referencedNamespace => BuildUsingStatement(referencedNamespace));
        }

        public static NamespaceDeclarationSyntax BuildNamespace(string namespaceName, TypeDeclarationSyntax containedTypeDeclaration)
        {
            // Currently limiting files to a single type, but this can be easily modified later
            // to be one or multiple
            // TODO: Maybe add some kind of "generated file info" comments at top of file?
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName)).AddMembers(containedTypeDeclaration);
        }

        public static string GetFileSyntaxAsString(NamespaceDeclarationSyntax namespaceDeclaration, IEnumerable<UsingDirectiveSyntax> usingDeclarations = null)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();
            if (usingDeclarations != null)
            {
                compilationUnit = compilationUnit.AddUsings(usingDeclarations.ToArray());
            }
            compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);

            return compilationUnit.NormalizeWhitespace().ToFullString();
        }

        public static BlockSyntax GetStatementsAsBlock(IEnumerable<StatementSyntax> statements)
        {
            return SyntaxFactory.Block(
                SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                SyntaxFactory.List(statements),
                SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }
    }
}
