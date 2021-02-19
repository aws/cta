using System;
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

    }
}
