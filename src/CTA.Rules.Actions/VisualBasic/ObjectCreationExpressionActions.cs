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
            ExpressionSyntax Action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                var newNode = SyntaxFactory.ParseExpression(newStatement).NormalizeWhitespace();
                return newNode;
            }
            return Action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectWithInvocationAction(string newExpression, string useParameters)
        {
            ExpressionSyntax Action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                bool.TryParse(useParameters, out var useParam);

                var newNode = SyntaxFactory.ParseExpression(newExpression) as InvocationExpressionSyntax;
                if (useParam)
                {
                    newNode = newNode.WithArgumentList(node.ArgumentList);
                }
                return newNode.NormalizeWhitespace();
            }
            return Action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceOrAddObjectPropertyIdentifierAction(string oldMember, string newMember, string newValueIfAdding = null)
        {
            ExpressionSyntax Action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                if (node.Initializer != null && node.Initializer.IsKind(SyntaxKind.ObjectMemberInitializer))
                {
                    var initializer = (ObjectMemberInitializerSyntax)node.Initializer;
                    var memberList = initializer.Initializers.Where(n => n.Kind() == SyntaxKind.NamedFieldInitializer).ToList();
                    if (memberList.Any())
                    {
                        var assignMemberList = memberList.Select(n => (NamedFieldInitializerSyntax)n);
                        var member = assignMemberList.FirstOrDefault(n => n.Name.ToFullString().Contains(oldMember));
                        SeparatedSyntaxList<FieldInitializerSyntax> newInitializers;
                        if (member != null)
                        {
                            var newExpression = SyntaxFactory.NamedFieldInitializer((IdentifierNameSyntax)syntaxGenerator.IdentifierName(newMember), member.Expression);
                            newInitializers = initializer.Initializers.Replace(member, newExpression);
                        }
                        else
                        {
                            var newExpression = SyntaxFactory.NamedFieldInitializer((IdentifierNameSyntax)syntaxGenerator.IdentifierName(newMember), (IdentifierNameSyntax)syntaxGenerator.IdentifierName(newValueIfAdding));
                            newInitializers = initializer.Initializers.Add(newExpression);
                        }
                        var newInitializer = initializer.WithInitializers(newInitializers);
                        node = node.WithInitializer(newInitializer).NormalizeWhitespace();
                    }
                }
                return node;
            }
            return Action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetReplaceObjectPropertyValueAction(string oldMember, string newMember)
        {
            ExpressionSyntax Action(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                if (node.Initializer != null && node.Initializer.IsKind(SyntaxKind.ObjectMemberInitializer))
                {
                    var initializer = (ObjectMemberInitializerSyntax)node.Initializer;
                    var memberList = initializer.Initializers.Where(n => n.Kind() == SyntaxKind.NamedFieldInitializer).ToList();
                    if (memberList.Any())
                    {
                        var assignMemberList = memberList.Select(n => (NamedFieldInitializerSyntax)n);
                        var member = assignMemberList.FirstOrDefault(n => n.Expression.ToFullString().Contains(oldMember));
                        if (member != null)
                        {
                            var right = SyntaxFactory.ParseExpression(member.Expression.ToFullString().Replace(oldMember, newMember));
                            var newExpression = SyntaxFactory.NamedFieldInitializer(member.Name, right);
                            var newInitializers = initializer.Initializers.Replace(member, newExpression);
                            var newInitializer = initializer.WithInitializers(newInitializers);
                            node = node.WithInitializer(newInitializer).NormalizeWhitespace();
                        }
                    }
                }
                return node;
            }
            return Action;
        }

        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ObjectCreationExpressionSyntax> GetAddCommentAction(string comment)
        {
            ObjectCreationExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, ObjectCreationExpressionSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, string.Format(Constants.VbCommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }

    }
}
