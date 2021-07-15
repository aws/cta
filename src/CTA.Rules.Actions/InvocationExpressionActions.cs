using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on Invocation Expressions
    /// </summary>
    public class InvocationExpressionActions
    {
        /// <summary>
        /// This Method replaces the entire expression including the parameters. For example, Math.Round(5.5) invocation expression matching on Round with a newMethod parameter of Abs(9.5) would return Abs(9.5) not Math.Abs(9.5).
        /// Please ensure to include the prefix part of the matching invocation expression since this method will replace it and the parameters as well.
        /// </summary>
        /// <param name="newMethod">The new invocation expression including the method to replace with and the parameters.</param>
        /// <param name="newParameters">The new invocation expression parameters to replace the old parameters with.</param>
        /// <returns></returns>
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetReplaceMethodWithObjectAndParametersAction(string newMethod, string newParameters)
        {
            //TODO what's the outcome if newMethod doesn't have a valid signature.. are there any options we could provide to parseexpression ?
            InvocationExpressionSyntax ReplaceMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                node = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.IdentifierName(newMethod),
                        SyntaxFactory.ParseArgumentList(newParameters))
                    .NormalizeWhitespace();
                return node;
            }
            return ReplaceMethod;
        }

        /// <summary>
        /// This Method replaces the entire expression up to the matching invocation expression. For example, Math.Round(5.5) invocation expression matching on Round with a newMethod parameter of Abs would return Abs(5.5) not Math.Abs(5.5).
        /// Please ensure to include the prefix part of the matching invocation expression since this method will replace it.
        /// </summary>
        /// <param name="newMethod">The new invocation expression including the method to replace with.</param>
        /// <returns></returns>
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetReplaceMethodWithObjectAction(string newMethod)
        {
            //TODO what's the outcome if newMethod doesn't have a valid signature.. are there any options we could provide to parseexpression ?
            InvocationExpressionSyntax ReplaceMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                node = node.WithExpression(SyntaxFactory.ParseExpression(newMethod)).NormalizeWhitespace(); 
                return node;
            }
            return ReplaceMethod;
        }

        /// <summary>
        /// This Method replaces the entire expression up to the matching invocation expression. It also adds the type from the typed argument list to the list of arguments.
        /// Example: DependencyResolver.Current.GetService<object>() becomes DependencyResolver.Current.GetService(typeof(object))
        /// </summary>
        /// <param name="newMethod">The new invocation expression including the method to replace with.</param>
        /// <returns></returns>
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetReplaceMethodWithObjectAddTypeAction(string newMethod)
        {
            InvocationExpressionSyntax ReplaceMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                var typeToAdd = node.Expression.DescendantNodes()?.OfType<GenericNameSyntax>()?.FirstOrDefault()?.TypeArgumentList;

                if(typeToAdd != null)
                {
                    var argumentList = SyntaxFactory.ParseArgumentList($"(typeof({typeToAdd.Arguments.ToString()}))");
                    node = node.WithArgumentList(argumentList);
                }

                node = node.WithExpression(SyntaxFactory.ParseExpression(newMethod)).NormalizeWhitespace();
                return node;
            }
            return ReplaceMethod;
        }

        /// <summary>
        /// This Method replaces only matching method in the invocation expression and its parameters.
        /// For example, Math.Round(5.5) invocation expression matching on Round with a newMethod parameter of Abs and an oldMethod parameter of Round with parameters of 8.5 would return Math.Abs(8.5).
        /// </summary>
        /// <param name="oldMethod">The matching method in the invocation expression to be replaced.</param>
        /// <param name="newMethod">The new method to replace the old method with in the invocation expression.</param>
        /// <param name="newParameters">The new invocation expression parameters to replace the old parameters with.</param>
        /// <returns></returns>
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetReplaceMethodAndParametersAction(string oldMethod, string newMethod, string newParameters)
        {
            //TODO what's the outcome if newMethod doesn't have a valid signature.. are there any options we could provide to parseexpression ?
            InvocationExpressionSyntax ReplaceOnlyMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                node = node.WithExpression(SyntaxFactory.ParseExpression(node.Expression.ToString().Replace(oldMethod, newMethod))).WithArgumentList(SyntaxFactory.ParseArgumentList(newParameters)).NormalizeWhitespace();
                return node;
            }
            return ReplaceOnlyMethod;
        }

        /// <summary>
        /// This Method replaces only matching method in the invocation expression. For example, Math.Round(5.5) invocation expression matching on Round with a newMethod parameter of Abs and an oldMethod parameter of Round would return Math.Abs(5.5).
        /// </summary>
        /// <param name="oldMethod">The matching method in the invocation expression to be replaced.</param>
        /// <param name="newMethod">The new method to replace the old method with in the invocation expression.</param>
        /// <returns></returns>
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetReplaceMethodOnlyAction(string oldMethod, string newMethod)
        {
            //TODO what's the outcome if newMethod doesn't have a valid signature.. are there any options we could provide to parseexpression ?
            InvocationExpressionSyntax ReplaceOnlyMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                node = node.WithExpression(SyntaxFactory.ParseExpression(node.Expression.ToString().Replace(oldMethod, newMethod))).NormalizeWhitespace();
                return node;
            }
            return ReplaceOnlyMethod;
        }

        /// <summary>
        /// This Method replaces only the parameters in the invocation expression. For example, Math.Round(5.5) invocation expression matching on Round with a parameter of (8) would return Math.Abs(8).
        /// </summary>
        /// <param name="newParameters">The new invocation expression parameters to replace the old parameters with.</param>
        /// <returns></returns>
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetReplaceParametersOnlyAction(string newParameters)
        {
            //TODO what's the outcome if newMethod doesn't have a valid signature.. are there any options we could provide to parseexpression ?
            InvocationExpressionSyntax ReplaceOnlyMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                node = node.WithArgumentList(SyntaxFactory.ParseArgumentList(newParameters)).NormalizeWhitespace();
                return node;
            }
            return ReplaceOnlyMethod;
        }

        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetAppendMethodAction(string appendMethod)
        {
            InvocationExpressionSyntax ReplaceMethod(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
            {
                node = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    node,
                    SyntaxFactory.IdentifierName(SyntaxFactory.ParseName(appendMethod).ToString())),
                SyntaxFactory.ArgumentList()).NormalizeWhitespace();
                return node;
            }
            return ReplaceMethod;
        }

        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetAddCommentAction(string comment)
        {
            InvocationExpressionSyntax AddComment(SyntaxGenerator syntaxGenerator, InvocationExpressionSyntax node)
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
