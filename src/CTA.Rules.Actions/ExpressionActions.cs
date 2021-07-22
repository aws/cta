using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions
{
    public class ExpressionActions
    {
        /// <summary>
        /// This Method adds an await operator to the invocation expression. For example, Math.Round(5.5) invocation expression would return await Math.Abs(5.5).
        /// </summary>
        /// <returns></returns>
        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetAddAwaitOperatorAction(string _)
        {
            SyntaxNode AddAwaitOperator(SyntaxGenerator syntaxGenerator, SyntaxNode node)
            {
                AwaitExpressionSyntax newNode = SyntaxFactory.AwaitExpression(SyntaxFactory.ParseExpression(node.WithoutTrivia().NormalizeWhitespace().ToFullString())); // SyntaxFactory.AwaitExpression().NormalizeWhitespace();
                newNode = newNode.WithTriviaFrom(node).NormalizeWhitespace();
                return SyntaxFactory.ExpressionStatement(newNode);
            }
            return AddAwaitOperator;
        }

        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetAddCommentAction(string comment)
        {
            SyntaxNode AddComment(SyntaxGenerator syntaxGenerator, SyntaxNode node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
    }
}
