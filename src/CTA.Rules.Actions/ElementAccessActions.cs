using System;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Update
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
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> GetReplaceElementAccessAction(string newExpression)
        {
            ElementAccessExpressionSyntax ReplaceElement(SyntaxGenerator syntaxGenerator, ElementAccessExpressionSyntax node)
            {
                var newNode = SyntaxFactory.ElementAccessExpression(SyntaxFactory.ParseExpression(newExpression).NormalizeWhitespace(), node.ArgumentList);
                newNode = newNode.NormalizeWhitespace();
                return newNode;
            }
            return ReplaceElement;
        }
    }
}
