using System;
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
        public const string AppUseMiddlewareTextTemplate = "{0} = {0}.UseMiddleware<{1}>();";
        public const string OriginMiddlewareCommentTemplate = "This middleware was generated from the {0} application lifecycle event";
        // The line that should be executed to trigger the next middleware
        public static string RequestDelegateInvokeText =
            $"await {RuntimeInjectable.RequestDelegateInjectable.PrivateFieldName}.Invoke({RuntimeInjectable.HttpContextInjectable.ParamName});";

        // A shortcut method to make building the entire class;
        // more hassle free than assembling each of the components
        // individually (which is possible if specialized functionality
        // is needed, but not recommended)
        public static ClassDeclarationSyntax BuildMiddlewareClass(
            string middlewareClassName,
            // Whether or not this middleware will invoke the next
            // if it exists, usually true, but for http handler
            // derived middleware this will be false
            bool shouldContinueAfterInvoke = true,
            // Statements to put after required statements in constructor
            IEnumerable<StatementSyntax> constructorAdditionalStatements = null,
            // Statements to put before next() call in invoke method,
            // statements here are hooked to a pre-request-handle lifecycle
            // event
            IEnumerable<StatementSyntax> preHandleStatements = null,
            // Statements to put before next() call in invoke method,
            // statements here are hooked to a post-request-handle lifecycle
            // event
            IEnumerable<StatementSyntax> postHandleStatements = null,
            // Fields that were used in source http handler/module
            IEnumerable<FieldDeclarationSyntax> additionalFieldDeclarations = null,
            // Properties that were used in source http handler/module
            IEnumerable<PropertyDeclarationSyntax> additionalPropertyDeclarations = null,
            // Methods in addition to invoke method in source http handler/module
            IEnumerable<MethodDeclarationSyntax> additionalMethodDeclarations = null)
        {
            var result = SyntaxFactory.ClassDeclaration(middlewareClassName).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            result = result.AddMembers(AddMiddlewareFields(additionalFieldDeclarations).ToArray());

            if (additionalPropertyDeclarations != null)
            {
                result = result.AddMembers(additionalPropertyDeclarations.ToArray());
            }

            result = result.AddMembers(
                    BuildMiddlewareConstructor(middlewareClassName, constructorAdditionalStatements),
                    BuildMiddlewareInvokeMethod(shouldContinueAfterInvoke, preHandleStatements, postHandleStatements));

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

        public static ConstructorDeclarationSyntax BuildMiddlewareConstructor(
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

        public static MethodDeclarationSyntax BuildMiddlewareInvokeMethod(
            bool shouldContinue = true,
            IEnumerable<StatementSyntax> preHandleStatements = null,
            IEnumerable<StatementSyntax> postHandleStatements = null)
        {
            IEnumerable<StatementSyntax> statements = preHandleStatements ?? new List<StatementSyntax>();

            // In most cases, shouldContinue will be true, but if
            // the middleware was generated from an http handler,
            // then this will not be the case
            if (shouldContinue)
            {
                // Ensure that call to next occurs between
                // pre-handle statements and post-handle
                // statements if any exist (usually exactly
                // one will)
                statements = statements.Append(SyntaxFactory.ParseStatement(RequestDelegateInvokeText));
            }

            if (postHandleStatements != null)
            {
                statements = statements.Concat(postHandleStatements);
            }

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(MiddlewareInvokeMethodReturnType), MiddlewareInvokeMethodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                // Accept runtime injected http context parameter
                .AddParameterListParameters(RuntimeInjectable.HttpContextInjectable.AsParameter)
                .WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));
        }

        public static StatementSyntax BuildMiddlewareRegistrationSyntax(string middlewareName, string originLifecycleHook = null)
        {
            var statement = SyntaxFactory.ParseStatement(string.Format(AppUseMiddlewareTextTemplate, RuntimeInjectable.AppBuilderInjectable.ParamName, middlewareName));

            if (originLifecycleHook != null)
            {
                statement = statement.AddComment(string.Format(OriginMiddlewareCommentTemplate, originLifecycleHook));
            }

            return statement;
        }
    }
}
