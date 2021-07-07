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
        public Func<SyntaxGenerator, ExpressionSyntax, ExpressionSyntax> GetAddAwaitOperatorAction(string _)
        {
            ExpressionSyntax AddAwaitOperator(SyntaxGenerator syntaxGenerator, ExpressionSyntax node)
            {
                return SyntaxFactory.AwaitExpression(node).NormalizeWhitespace();
            }
            return AddAwaitOperator;
        }

        public Func<SyntaxGenerator, ExpressionSyntax, ExpressionSyntax> GetAddCommentAction(string comment)
        {
            ExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, ExpressionSyntax node)
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
