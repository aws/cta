using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.VisualBasic
{
    /// <summary>
    /// List of actions that can run on Identifier Names
    /// </summary>
    public class IdentifierNameActions
    {
        public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetReplaceIdentifierAction(string identifierName)
        {
            IdentifierNameSyntax ReplaceIdentifier(SyntaxGenerator syntaxGenerator, IdentifierNameSyntax node)
            {
                var leadingTrivia = node.GetLeadingTrivia();
                var trailingTrivia = node.GetTrailingTrivia();
                node = node.WithIdentifier(SyntaxFactory.Identifier(identifierName)).NormalizeWhitespace();
                node = node.WithLeadingTrivia(leadingTrivia);
                node = node.WithTrailingTrivia(trailingTrivia);
                return node;
            }
            return ReplaceIdentifier;
        }

        public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetReplaceIdentifierInsideClassAction(string identifier, string classFullKey)
        {
            IdentifierNameSyntax ReplaceIdentifier2(SyntaxGenerator syntaxGenerator, IdentifierNameSyntax node)
            {
                var currentNode = node.Parent;
                while (currentNode != null && currentNode.GetType() != typeof(ClassBlockSyntax))
                {
                    currentNode = currentNode.Parent;
                }
                var classNode = currentNode as ClassBlockSyntax;

                while (currentNode != null && currentNode.GetType() != typeof(NamespaceBlockSyntax))
                {
                    currentNode = currentNode.Parent;
                }

                if (classNode == null || !(currentNode is NamespaceBlockSyntax namespaceNode)) { return node; }

                var fullName = string.Concat(namespaceNode.NamespaceStatement.Name, ".", classNode.ClassStatement.Identifier.Text);

                if (fullName == classFullKey)
                {
                    var leadingTrivia = node.GetLeadingTrivia();
                    var trailingTrivia = node.GetTrailingTrivia();
                    node = node.WithIdentifier(SyntaxFactory.Identifier(identifier)).NormalizeWhitespace();
                    node = node.WithLeadingTrivia(leadingTrivia);
                    node = node.WithTrailingTrivia(trailingTrivia);
                    return node;
                }
                else
                {
                    return node;
                }

            }
            return ReplaceIdentifier2;
        }
    }
}
