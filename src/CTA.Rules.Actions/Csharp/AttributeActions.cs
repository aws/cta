﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.Csharp
{
    /// <summary>
    /// List of actions that can run on attributes
    /// </summary>
    public class AttributeActions
    {
        public Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax> GetChangeAttributeAction(string attributeName)
        {
            AttributeSyntax ChangeAttribute(SyntaxGenerator syntaxGenerator, AttributeSyntax node)
            {
                node = node.WithName(SyntaxFactory.ParseName(attributeName)).NormalizeWhitespace();
                return node;
            }
            return ChangeAttribute;
        }
    }
}
