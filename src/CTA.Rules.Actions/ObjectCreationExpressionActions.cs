using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions
{
    public class ObjectCreationExpressionActions
    {
        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectinitializationAction(string newStatement)
        {
            Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> action = (SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node) =>
            {
                var newNode = SyntaxFactory.ParseExpression(newStatement).NormalizeWhitespace();
                return newNode;
            };
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectWithInvocationAction(string NewExpression, string UseParameters)
        {
            Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> action = (SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node) =>
            {
                var useParam = false;
                bool.TryParse(UseParameters, out useParam);

                var newNode = SyntaxFactory.ParseExpression(NewExpression) as InvocationExpressionSyntax;
                if (useParam)
                {
                    newNode = newNode.WithArgumentList(node.ArgumentList);
                }
                return newNode.NormalizeWhitespace();
            };
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceOrAddObjectPropertyIdentifierAction(string oldMember, string newMember, string newValueIfAdding)
        {
            Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> action = (SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node) =>
            {
                var member = node.Initializer.Expressions.FirstOrDefault(n => n.ToFullString().Contains(oldMember));
                if(member != null)
                {
                    var newExpression = SyntaxFactory.ParseExpression(member.ToFullString().Replace(oldMember,newMember));
                    var newNode = node.Initializer.Expressions.Replace(member, newExpression);
                    node = node.WithInitializer(node.Initializer.WithExpressions(newNode)).NormalizeWhitespace();
                }
                else
                {
                    var newExpression = SyntaxFactory.ParseExpression(newMember + "=" + newValueIfAdding);
                    var newNode = node.Initializer.Expressions.Add(newExpression);
                    node = node.WithInitializer(node.Initializer.WithExpressions(newNode)).NormalizeWhitespace();
                }
                return node;
            };
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectIdentifierAction(string identifier)
        {
            Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> action = (SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node) =>
            {
                node = node.WithType(SyntaxFactory.ParseTypeName(identifier)).NormalizeWhitespace();
                return node;
            };
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ObjectCreationExpressionSyntax> GetAddCommentAction(string comment)
        {
            ObjectCreationExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
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
