﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.Csharp
{
    //TODO shouldn't this be inside ClassActions ?
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

        public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetReplaceIdentifierInsideClassAction(string Identifier, string ClassFullKey)
        {
            IdentifierNameSyntax ReplaceIdentifier2(SyntaxGenerator syntaxGenerator, IdentifierNameSyntax node)
            {
                var currentNode = node.Parent;
                while (currentNode != null && currentNode.GetType() != typeof(ClassDeclarationSyntax))
                {
                    currentNode = currentNode.Parent;
                }
                var classNode = currentNode as ClassDeclarationSyntax;

                while (currentNode != null && currentNode.GetType() != typeof(NamespaceDeclarationSyntax))
                {
                    currentNode = currentNode.Parent;
                }

                if (classNode == null || !(currentNode is NamespaceDeclarationSyntax namespaceNode)) { return node; }

                var fullName = string.Concat(namespaceNode.Name, ".", classNode.Identifier.Text);

                if (fullName == ClassFullKey)
                {
                    var leadingTrivia = node.GetLeadingTrivia();
                    var trailingTrivia = node.GetTrailingTrivia();
                    node = node.WithIdentifier(SyntaxFactory.Identifier(Identifier)).NormalizeWhitespace();
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
