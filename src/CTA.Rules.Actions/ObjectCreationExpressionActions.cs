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
            ExpressionSyntax action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                var newNode = SyntaxFactory.ParseExpression(newStatement).NormalizeWhitespace();
                return newNode;
            }
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectWithInvocationAction(string NewExpression, string UseParameters)
        {
            ExpressionSyntax action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                bool.TryParse(UseParameters, out var useParam);

                var newNode = SyntaxFactory.ParseExpression(NewExpression) as InvocationExpressionSyntax;
                if (useParam)
                {
                    newNode = newNode.WithArgumentList(node.ArgumentList);
                }
                return newNode.NormalizeWhitespace();
            }
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceOrAddObjectPropertyIdentifierAction(string oldMember, string newMember, string newValueIfAdding)
        {
            ExpressionSyntax action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                if(node.Initializer != null && node.Initializer.Expressions.Count > 0)
                {
                    var memberList = node.Initializer.Expressions.Where(n => n.Kind() == SyntaxKind.SimpleAssignmentExpression);
                    if(memberList.Count() > 0)
                    {
                        var assignMemberList = memberList.Select(n => (AssignmentExpressionSyntax)n);
                        var member = assignMemberList.Where(n => n.Left.ToFullString().Contains(oldMember)).FirstOrDefault();
                        if (member != null)
                        {
                            var newExpression = SyntaxFactory.ParseExpression(newMember + "=" + member.Right);
                            var newNode = node.Initializer.Expressions.Replace(member, newExpression);
                            node = node.WithInitializer(node.Initializer.WithExpressions(newNode)).NormalizeWhitespace();
                        }
                        else
                        {
                            var newExpression = SyntaxFactory.ParseExpression(newMember + "=" + newValueIfAdding);
                            var newNode = node.Initializer.Expressions.Add(newExpression);
                            node = node.WithInitializer(node.Initializer.WithExpressions(newNode)).NormalizeWhitespace();
                        }
                    }
                }
                return node;
            }
            return action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectPropertyValueAction(string oldMember, string newMember)
        {
            ExpressionSyntax action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                if (node.Initializer != null && node.Initializer.Expressions.Count > 0)
                {
                    var memberList = node.Initializer.Expressions.Where(n => n.Kind() == SyntaxKind.SimpleAssignmentExpression);
                    if (memberList.Count() > 0)
                    {
                        var assignMemberList = memberList.Select(n => (AssignmentExpressionSyntax)n);
                        var member = assignMemberList.Where(n => n.Right.ToFullString().Contains(oldMember)).FirstOrDefault();
                        if (member != null)
                        {
                            var newExpression = SyntaxFactory.ParseExpression(member.Left + "=" + member.Right.ToString().Replace(oldMember,newMember));
                            var newNode = node.Initializer.Expressions.Replace(member, newExpression);
                            node = node.WithInitializer(node.Initializer.WithExpressions(newNode)).NormalizeWhitespace();
                        }
                    }
                }
                return node;
            }
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
