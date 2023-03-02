using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Editing;
using CTA.Rules.Actions.ActionHelpers;

namespace CTA.Rules.Actions.VisualBasic
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
                var newNode = SyntaxFactory.AwaitExpression(
                    SyntaxFactory.ParseExpression(node.WithoutTrivia().ToFullString()))
                    .NormalizeWhitespace();
                newNode = newNode.WithTriviaFrom(node);
                return SyntaxFactory.ExpressionStatement(newNode);
            }
            return AddAwaitOperator;
        }

        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetAddCommentAction(string comment)
        {
            SyntaxNode AddComment(SyntaxGenerator syntaxGenerator, SyntaxNode node)
            {
                return CommentHelper.AddVBComment(node, comment);
            }
            return AddComment;
        }
    }
}
