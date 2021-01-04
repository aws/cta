using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;

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
    }
}
