using System;
using System.Collections.Generic;
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
            MethodDeclarationSyntax AddComment(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
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

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetChangeMethodNameAction(string newMethodName)
        {
            MethodDeclarationSyntax ChangeMethodName(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
               var newMethodNode = node;
               newMethodNode = newMethodNode.WithIdentifier(SyntaxFactory.Identifier(newMethodName)).NormalizeWhitespace();
               return newMethodNode;
            }
            return ChangeMethodName;
        }

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetChangeMethodToReturnTaskTypeAction(string newMethodName)
        {
            MethodDeclarationSyntax ChangeMethodToReturnTaskType(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
                TypeSyntax asyncReturnType;

                if (node.ReturnType.ToFullString().Trim().Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    asyncReturnType = SyntaxFactory.IdentifierName("Task").WithTrailingTrivia(SyntaxFactory.Space);
                }
                else
                {
                    var currentTrivia = node.ReturnType.GetTrailingTrivia();
                    asyncReturnType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task")).WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(node.ReturnType.WithoutTrailingTrivia()))).WithTrailingTrivia(currentTrivia);
                }

                var newMethodNode = node.WithReturnType(asyncReturnType);
                return newMethodNode;
            }
            return ChangeMethodToReturnTaskType;
        }

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetRemoveMethodParametersAction()
        {
            MethodDeclarationSyntax RemoveMethodParametersAction(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
                List<ParameterSyntax> parameters = new List<ParameterSyntax>();
                var newMethodNode = node.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters))).NormalizeWhitespace();
                return newMethodNode;
            }
            return RemoveMethodParametersAction;
        }

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetAddParametersToMethodAction(string types, string identifiers)
        {
            MethodDeclarationSyntax AddParametersToMethodAction(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
                var newMethodNode = node;
                if (!string.IsNullOrWhiteSpace(identifiers) && !string.IsNullOrWhiteSpace(types))
                {
                    var identifiersArray = identifiers.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var typesArray = types.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (identifiersArray.Length == typesArray.Length)
                    {
                        List<ParameterSyntax> parameters = new List<ParameterSyntax>();
                        for (int i = 0; i < identifiersArray.Length; i++)
                        {
                            parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(identifiersArray[i])).WithType(SyntaxFactory.ParseTypeName(typesArray[i])));
                        }
                        newMethodNode = node.AddParameterListParameters(parameters.ToArray());
                    }
                };
                return newMethodNode;
            }
            return AddParametersToMethodAction;
        }

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetCommentMethodAction(string comment = null)
        {
            MethodDeclarationSyntax CommentMethodAction(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
                var startComment = SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "/*");
                var endComment = SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "*/");

                var newMethodNode = node.WithLeadingTrivia(new SyntaxTriviaList(startComment)).WithTrailingTrivia(new SyntaxTriviaList(endComment));
                node = node.ReplaceNode(node, newMethodNode);

                if (!string.IsNullOrWhiteSpace(comment))
                {
                    var addCommentsToMethodFunc = GetAddCommentAction(comment);
                    return addCommentsToMethodFunc(syntaxGenerator, node);
                }
                return node;
            }
            return CommentMethodAction;
        }

        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetAddExpressionToMethodAction(string expression)
        {
            MethodDeclarationSyntax AddExpressionToMethodAction(SyntaxGenerator syntaxGenerator, MethodDeclarationSyntax node)
            {
                var newMethodNode = node;
                StatementSyntax parsedExpression = SyntaxFactory.ParseStatement(expression);
                if (!parsedExpression.FullSpan.IsEmpty)
                {
                    newMethodNode = node.AddBodyStatements(new StatementSyntax[] { parsedExpression }).NormalizeWhitespace();
                }
                return newMethodNode;
            }
            return AddExpressionToMethodAction;
        }
    }
}
