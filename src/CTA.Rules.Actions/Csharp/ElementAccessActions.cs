using System;
using CTA.Rules.Actions.ActionHelpers;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Update.Csharp
{
    /// <summary>
    /// List of actions that can run on Element Accesses
    /// </summary>
    public class ElementAccessActions
    {
        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> GetAddCommentAction(string comment)
        {
            ElementAccessExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, ElementAccessExpressionSyntax node)
            {
                return (ElementAccessExpressionSyntax)CommentHelper.AddCSharpComment(node, comment);
            }
            return AddComment;
        }

        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> GetReplaceElementAccessAction(string newExpression)
        {
            ElementAccessExpressionSyntax ReplaceElement(SyntaxGenerator syntaxGenerator, ElementAccessExpressionSyntax node)
            {
                var newNode = SyntaxFactory.ElementAccessExpression(SyntaxFactory.ParseExpression(newExpression), node.ArgumentList);
                newNode = newNode.NormalizeWhitespace();
                return newNode;
            }
            return ReplaceElement;
        }
    }
}
