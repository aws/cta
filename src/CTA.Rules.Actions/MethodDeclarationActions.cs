using System;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on Method Declarations
    /// </summary>
    public class MethodDeclarationActions
    {
        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetAddCommentAction(string comment)
        {
            Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> AddComment = (SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node) =>
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            };
            return AddComment;
        }

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetAppendExpressionAction(string expression)
        {
            // TODO: This will add an expression at the bottom of a method body, in the future we should add granularity for where to add the expression within a method body
            Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> AppendExpression = (SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node) =>
            {
                StatementSyntax statementExpression = SyntaxFactory.ParseStatement(expression);
                if(!statementExpression.FullSpan.IsEmpty)
                {
                    BlockSyntax nodeBody = node.Body;
                    nodeBody = nodeBody.AddStatements(statementExpression);
                    node = node.WithBody(nodeBody).NormalizeWhitespace();
                }
                return node;
            };
            return AppendExpression;
        }
    }
}
