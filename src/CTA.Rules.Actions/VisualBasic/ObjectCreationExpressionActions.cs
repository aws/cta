using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis;
using CTA.Rules.Config;


namespace CTA.Rules.Actions.VisualBasic
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

        public Func<SyntaxGenerator, ObjectMemberInitializerSyntax, ObjectMemberInitializerSyntax> GetReplaceOrAddObjectPropertyIdentifierAction(string oldMember, string newMember, string newValueIfAdding = null)
        {
            ObjectMemberInitializerSyntax action(SyntaxGenerator syntaxGenerator, ObjectMemberInitializerSyntax node)
            {
                if (node.Initializers.Count > 0)
                {
                    var memberList = node.Initializers.Where(n => n.Kind() == SyntaxKind.NamedFieldInitializer);
                    if (memberList.Count() > 0)
                    {
                        var assignMemberList = memberList.Select(n => (NamedFieldInitializerSyntax)n);
                        var member = assignMemberList.Where(n => n.Name.ToFullString().Contains(oldMember)).FirstOrDefault();
                        if (member != null)
                        {
                            var newExpression = SyntaxFactory.NamedFieldInitializer((IdentifierNameSyntax)syntaxGenerator.IdentifierName(newMember), member.Expression);
                            var newNode = node.Initializers.Replace(member, newExpression);
                            node = node.WithInitializers(newNode).NormalizeWhitespace();
                        }
                        else
                        {
                            var newExpression = SyntaxFactory.NamedFieldInitializer((IdentifierNameSyntax)syntaxGenerator.IdentifierName(newMember), (IdentifierNameSyntax)syntaxGenerator.IdentifierName(newValueIfAdding));
                            var newNode = node.Initializers.Add(newExpression);
                            node = node.WithInitializers(newNode).NormalizeWhitespace();
                        }
                    }
                }
                return node;
            }
            return action;
        }

        public Func<SyntaxGenerator, ObjectMemberInitializerSyntax, ObjectMemberInitializerSyntax> GetReplaceObjectPropertyValueAction(string oldMember, string newMember)
        {
            ObjectMemberInitializerSyntax action(SyntaxGenerator syntaxGenerator, ObjectMemberInitializerSyntax node)
            {
                if (node.Initializers.Count > 0)
                {
                    var memberList = node.Initializers.Where(n => n.Kind() == SyntaxKind.NamedFieldInitializer);
                    if (memberList.Count() > 0)
                    {
                        var assignMemberList = memberList.Select(n => (NamedFieldInitializerSyntax)n);
                        var member = assignMemberList.Where(n => n.Expression.ToFullString().Contains(oldMember)).FirstOrDefault();
                        if (member != null)
                        {
                            var right = SyntaxFactory.ParseExpression(member.Expression.ToFullString().Replace(oldMember, newMember));
                            var newExpression = SyntaxFactory.NamedFieldInitializer(member.Name, right);
                            var newNode = node.Initializers.Replace(member, newExpression);
                            node = node.WithInitializers(newNode).NormalizeWhitespace();
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
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }

    }
}
