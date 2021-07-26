using System.Collections.Generic;
using System.Linq;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Helpers
{
    public static class MiddlewareSyntaxHelper
    {
        public const string MiddlewareInvokeMethodName = "Invoke";
        public const string MiddlewareInvokeMethodReturnType = "Task";
        public const string MiddlewareLambdaRegistrationMethodName = "Use";
        public const string MiddlewareLambdaNextCallTemplate = "await {0}();";
        public const string AppUseMiddlewareTextTemplate = "{0}.UseMiddleware<{1}>();";
        public const string FromHandlerComment = "This middleware was originally an http handler, use conditions must be added manually";

        public static IEnumerable<string> RequiredNamespaces => new[]
        {
            "Microsoft.AspNetCore.Http",
            "System.Threading.Tasks"
        };

        /// <summary>
        /// The line that should be executed to trigger the next middleware
        /// </summary>
        public static string RequestDelegateInvokeText =
            $"await {RuntimeInjectable.RequestDelegateInjectable.PrivateFieldName}.Invoke({RuntimeInjectable.HttpContextInjectable.ParamName});";

        /// <summary>
        /// A shortcut method to make building the entire class;
        /// more hassle free than assembling each of the components
        /// individually (which is possible if specialized functionality
        /// is needed, but not recommended)
        /// </summary>
        /// <param name="middlewareClassName">The name of the middleware class to be created</param>
        /// <param name="shouldContinueAfterInvoke">Whether or not this middleware will invoke the next
        /// if it exists, usually true, but for http handler derived middleware this will be false</param>
        /// <param name="constructorAdditionalStatements">Statements to put after required statements in constructor</param>
        /// <param name="preHandleStatements">Statements to put before next() call in invoke method,
        /// statements here are hooked to a pre-request-handle lifecycle event</param>
        /// <param name="postHandleStatements">Statements to put before next() call in invoke method,
        /// statements here are hooked to a post-request-handle lifecycle event</param>
        /// <param name="additionalFieldDeclarations">Fields that were used in source http handler/module</param>
        /// <param name="additionalPropertyDeclarations">Properties that were used in source http handler/module</param>
        /// <param name="additionalMethodDeclarations">Methods in addition to invoke method in source http handler/module</param>
        /// <returns>ClassDeclarationSyntax node for new middleware class</returns>
        public static ClassDeclarationSyntax ConstructMiddlewareClass(
            string middlewareClassName,
            bool shouldContinueAfterInvoke = true,
            IEnumerable<StatementSyntax> constructorAdditionalStatements = null,
            IEnumerable<StatementSyntax> preHandleStatements = null,
            IEnumerable<StatementSyntax> postHandleStatements = null,
            IEnumerable<FieldDeclarationSyntax> additionalFieldDeclarations = null,
            IEnumerable<PropertyDeclarationSyntax> additionalPropertyDeclarations = null,
            IEnumerable<MethodDeclarationSyntax> additionalMethodDeclarations = null)
        {
            var result = SyntaxFactory.ClassDeclaration(middlewareClassName).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            result = result.AddMembers(AddMiddlewareFields(additionalFieldDeclarations).ToArray());

            if (additionalPropertyDeclarations != null)
            {
                result = result.AddMembers(additionalPropertyDeclarations.ToArray());
            }

            result = result.AddMembers(
                    ConstructMiddlewareConstructor(middlewareClassName, constructorAdditionalStatements),
                    ConstructMiddlewareInvokeMethod(shouldContinueAfterInvoke, preHandleStatements, postHandleStatements));

            if (additionalMethodDeclarations != null)
            {
                result = result.AddMembers(additionalMethodDeclarations.ToArray());
            }

            return result;
        }

        public static IEnumerable<FieldDeclarationSyntax> AddMiddlewareFields(IEnumerable<FieldDeclarationSyntax> additionalDeclarations = null)
        {
            var newFields = new List<FieldDeclarationSyntax>()
            {
                // We guarantee that _next will be present as a private readonly field
                RuntimeInjectable.RequestDelegateInjectable.GetFieldDeclaration()
            };

            if (additionalDeclarations == null)
            {
                return newFields;
            }

            return newFields.UnionSyntaxNodeCollections(additionalDeclarations);
        }

        public static ConstructorDeclarationSyntax ConstructMiddlewareConstructor(
            string middlewareClassName,
            IEnumerable<StatementSyntax> additionalStatements = null)
        {
            var statements = new List<StatementSyntax>()
            {
                // We guarantee that request delegate will be assigned to _next
                RuntimeInjectable.RequestDelegateInjectable.PrivateFieldAssignment
            };

            if (additionalStatements != null)
            {
                statements.AddRange(additionalStatements);
            }

            return SyntaxFactory.ConstructorDeclaration(middlewareClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                // Accept runtime injected request delegate as next
                .AddParameterListParameters(RuntimeInjectable.RequestDelegateInjectable.AsParameter)
                .WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));
        }

        public static MethodDeclarationSyntax ConstructMiddlewareInvokeMethod(
            bool shouldContinue = true,
            IEnumerable<StatementSyntax> preHandleStatements = null,
            IEnumerable<StatementSyntax> postHandleStatements = null)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(MiddlewareInvokeMethodReturnType), MiddlewareInvokeMethodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                // Accept runtime injected http context parameter
                .AddParameterListParameters(RuntimeInjectable.HttpContextInjectable.AsParameter)
                .WithBody(ConstructMiddlewareMainContentBlock(RequestDelegateInvokeText, shouldContinue, preHandleStatements, postHandleStatements));
        }

        public static LambdaExpressionSyntax ConstructMiddlewareLambda(
            IEnumerable<StatementSyntax> preHandleStatements = null,
            IEnumerable<StatementSyntax> postHandleStatements = null) {

            return SyntaxFactory.ParenthesizedLambdaExpression()
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(RuntimeInjectable.HttpContextInjectable.ParamName)),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(RuntimeInjectable.RequestDelegateInjectable.ParamName)))
                .WithBlock(ConstructMiddlewareMainContentBlock(
                    string.Format(MiddlewareLambdaNextCallTemplate, RuntimeInjectable.RequestDelegateInjectable.ParamName),
                    true, preHandleStatements, postHandleStatements))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
        }

        public static BlockSyntax ConstructMiddlewareMainContentBlock(
            string nextCallText,
            bool shouldContinue = true,
            IEnumerable<StatementSyntax> preHandleStatements = null,
            IEnumerable<StatementSyntax> postHandleStatements = null)
        {
            IEnumerable<StatementSyntax> statements = preHandleStatements ?? new List<StatementSyntax>();

            // In most cases, shouldContinue will be true, but if the middleware was generated from
            // an http handler, then this will not be the case
            if (shouldContinue)
            {
                // Ensure that call to next occurs between pre-handle statements and post-handle
                // statements if any exist (usually exactly one will)
                statements = statements.Append(SyntaxFactory.ParseStatement(nextCallText));
            }

            if (postHandleStatements != null)
            {
                statements = statements.Concat(postHandleStatements);
            }

            return CodeSyntaxHelper.GetStatementsAsBlock(statements);
        }

        public static StatementSyntax ConstructMiddlewareRegistrationSyntax(string middlewareName, string originLifecycleHook = null, string partialOriginClass = null)
        {
            var statement = SyntaxFactory.ParseStatement(string.Format(AppUseMiddlewareTextTemplate, RuntimeInjectable.AppBuilderInjectable.ParamName, middlewareName));
            var comments = new List<string>();

            if (originLifecycleHook == WebFormsAppLifecycleEvent.RequestHandlerExecute.ToString())
            {
                comments.Add(FromHandlerComment);
            }
            else if (originLifecycleHook != null)
            {
                comments.Add(string.Format(Constants.NewEventRepresentationCommentTemplate, originLifecycleHook));
            }

            if (partialOriginClass != null)
            {
                comments.Add(string.Format(Constants.ClassSplitCommentTemplate, partialOriginClass));
            }

            // Use re-wrapping to wrap each comment line but keep a newline between each comment
            return comments.Count() > 0 ? statement.AddComment(comments, true, Constants.DefaultCommentLineCharacterLimit) : statement;
        }

        public static StatementSyntax ConstructMiddlewareLambdaRegistrationSyntax(LambdaExpressionSyntax middlewareLambda, string originLifecycleHook = null)
        {
            var argument = SyntaxFactory.Argument(middlewareLambda);
            var expression = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(RuntimeInjectable.AppBuilderInjectable.ParamName),
                    SyntaxFactory.IdentifierName(MiddlewareLambdaRegistrationMethodName)))
                .AddArgumentListArguments(argument);
            var statement = SyntaxFactory.ExpressionStatement(expression);

            if (originLifecycleHook != null)
            {
                return statement.AddComment(string.Format(Constants.NewEventRepresentationCommentTemplate, originLifecycleHook));
            }

            return statement;
        }
    }
}
