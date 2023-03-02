using System;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using CTA.Rules.Actions.ActionHelpers;

namespace CTA.Rules.Actions.VisualBasic
{
    /// <summary>
    /// List of actions that can run on Method Declarations
    /// </summary>
    public class MethodBlockActions
    {
        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetAddCommentAction(string comment, string dontUseCTAPrefix = null)
        {
            MethodBlockSyntax AddComment(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
            {
                return (MethodBlockSyntax)CommentHelper.AddVBComment(node, comment, dontUseCTAPrefix);
            }
            return AddComment;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetAppendExpressionAction(string expression)
        {
            // TODO: This will add an expression at the bottom of a method body, in the future we should add granularity for where to add the expression within a method body
            Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> appendExpression = (SyntaxGenerator syntaxGenerator, MethodBlockSyntax node) =>
            {
                StatementSyntax statementExpression = expression.Contains("Await")
                    ? ParseAwaitExpression(expression)
                    : SyntaxFactory.ParseExecutableStatement(expression);
                if (!statementExpression.FullSpan.IsEmpty)
                {
                    node = node.AddStatements(statementExpression).NormalizeWhitespace();
                }

                return node;
            };
            return appendExpression;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetChangeMethodNameAction(string newMethodName)
        {
            MethodBlockSyntax ChangeMethodName(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
            {
                var newMethodStatement = node.SubOrFunctionStatement.WithIdentifier(SyntaxFactory.Identifier(newMethodName));
                var newMethodNode = node.WithSubOrFunctionStatement(newMethodStatement).NormalizeWhitespace();
                return newMethodNode;
            }
            return ChangeMethodName;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetChangeMethodToReturnTaskTypeAction()
        {
            MethodBlockSyntax ChangeMethodToReturnTaskType(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
            {
                var methodStatement = node.SubOrFunctionStatement;
                var oldAsClause = methodStatement.AsClause;
                if (methodStatement.IsKind(SyntaxKind.SubStatement))
                {
                    // if sub, convert to function and return task
                    var functionStatement = SyntaxFactory.FunctionStatement(methodStatement.Identifier);
                    functionStatement =
                        functionStatement.WithAsClause(
                                SyntaxFactory.SimpleAsClause(SyntaxFactory.IdentifierName("Task")))
                            .WithModifiers(methodStatement.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));

                    var newNode = SyntaxFactory.MethodBlock(SyntaxKind.FunctionBlock,
                        functionStatement, node.Statements, SyntaxFactory.EndFunctionStatement());
                    return newNode.NormalizeWhitespace();
                }

                // already function, need to wrap return in task
                var genericName =
                    SyntaxFactory.GenericName("Task", SyntaxFactory.TypeArgumentList(oldAsClause.Type));
                methodStatement = methodStatement.WithAsClause(SyntaxFactory.SimpleAsClause(genericName))
                    .WithModifiers(methodStatement.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));
                return node.WithSubOrFunctionStatement(methodStatement).NormalizeWhitespace();
            }
            return ChangeMethodToReturnTaskType;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetRemoveMethodParametersAction()
        {
            MethodBlockSyntax RemoveMethodParametersAction(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
            {
                var parameters = new List<ParameterSyntax>();
                var newMethodStatement =
                    node.SubOrFunctionStatement
                        .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
                        .NormalizeWhitespace();
                return node.WithSubOrFunctionStatement(newMethodStatement).NormalizeWhitespace();
            }
            return RemoveMethodParametersAction;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetAddParametersToMethodAction(string types, string identifiers)
        {
            MethodBlockSyntax AddParametersToMethodAction(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
            {
                var newMethodStatement = node.SubOrFunctionStatement;
                if (!string.IsNullOrWhiteSpace(identifiers) && !string.IsNullOrWhiteSpace(types))
                {
                    var identifiersArray = identifiers.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var typesArray = types.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (identifiersArray.Length == typesArray.Length)
                    {
                        var parameters = new List<ParameterSyntax>();
                        for (var i = 0; i < identifiersArray.Length; i++)
                        {
                            parameters.Add(SyntaxFactory
                                .Parameter(SyntaxFactory.ModifiedIdentifier(identifiersArray[i]))
                                .WithAsClause(
                                    SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName(typesArray[i])))
                                .NormalizeWhitespace());
                        }

                        var separatedSyntaxList = SyntaxFactory.SeparatedList(parameters);
                        newMethodStatement =
                            newMethodStatement.WithParameterList(SyntaxFactory.ParameterList(separatedSyntaxList))
                                .NormalizeWhitespace();
                    }
                }

                return node.WithSubOrFunctionStatement(newMethodStatement).NormalizeWhitespace();
            }

            return AddParametersToMethodAction;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetCommentMethodAction(
            string comment = null,
            string dontUseCTAPrefix = null)
        {
             MethodBlockSyntax CommentMethodAction(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
             {
                 var bodyStatments = node.Statements;
                 var newBody = new SyntaxTriviaList();
                 foreach (var statement in bodyStatments)
                 {
                     var newLine = SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, @$"' {statement.ToFullString()}");
                     newBody = newBody.Add(newLine).Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, ""));
                 }

                 var endStatement = node.EndSubOrFunctionStatement.WithLeadingTrivia(newBody);
                 var newMethodNode = node.WithEndSubOrFunctionStatement(endStatement)
                     .WithStatements(new SyntaxList<StatementSyntax>());
                 
                 if (!string.IsNullOrWhiteSpace(comment))
                 {
                     var addCommentsToMethodFunc = GetAddCommentAction(comment, dontUseCTAPrefix);
                     return addCommentsToMethodFunc(syntaxGenerator, newMethodNode);
                 }
            
                 return newMethodNode.NormalizeWhitespace();
             }
            return CommentMethodAction;
        }

        public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetAddExpressionToMethodAction(string expression)
        {
            MethodBlockSyntax AddExpressionToMethodAction(SyntaxGenerator syntaxGenerator, MethodBlockSyntax node)
            {
                var newMethodNode = node;
                var parsedExpression = expression.Contains("Await")
                    ? ParseAwaitExpression(expression)
                    : SyntaxFactory.ParseExecutableStatement(expression);
                if (!parsedExpression.FullSpan.IsEmpty)
                {
                    var body = node.Statements;
                    int returnIndex = body.IndexOf(s => s.IsKind(SyntaxKind.ReturnStatement));
                    if (returnIndex >= 0)
                    {
                        // insert new statement before return
                        body = body.Insert(returnIndex, parsedExpression);
                        newMethodNode = node.WithStatements(body);
                    }
                    else
                    {
                        newMethodNode = node.AddStatements(parsedExpression);
                    }
                }

                return newMethodNode.NormalizeWhitespace();
            }

            return AddExpressionToMethodAction;
        }
        
        private StatementSyntax ParseAwaitExpression(string expression)
        {
            expression = expression.Replace("Await", "", StringComparison.OrdinalIgnoreCase);
            var parsedExpression = SyntaxFactory.ParseExpression(expression);
            var awaitExpression = SyntaxFactory.AwaitExpression(parsedExpression);
            return SyntaxFactory.ExpressionStatement(awaitExpression);
        }
    }
}
