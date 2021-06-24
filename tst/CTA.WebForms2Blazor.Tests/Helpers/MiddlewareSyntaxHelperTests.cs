using CTA.WebForms2Blazor.Helpers;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    public class MiddlewareSyntaxHelperTests
    {
        private const string MiddlewareClassName = "MyMiddleware";
        private const string ExpectedMiddlewareClassSignature = "public class MyMiddleware";
        private const string ExpectedMiddlewareConstructorSignature = "public MyMiddleware(RequestDelegate next)";
        private const string ExpectedMiddlewareInvokeSignature = "public async Task Invoke(HttpContext context)";

        private const string ExpectedDelegateConstructorAssignmentText = "_next = next;";
        private const string ExpectedDelegateInvokeCallText = "await _next.Invoke(context);";
        private const string ExpectedDelegateFieldDeclarationText = "private readonly RequestDelegate _next;";

        private static string ExpectedMiddlwareRegistrationString => $"app = app.UseMiddleware<{MiddlewareClassName}>();";
        private static string ExpectedBasicMiddlewareClassText =>
$@"{ExpectedMiddlewareClassSignature}
{{
    {ExpectedDelegateFieldDeclarationText}
    {ExpectedMiddlewareConstructorSignature}
    {{
        {ExpectedDelegateConstructorAssignmentText}
    }}

    {ExpectedMiddlewareInvokeSignature}
    {{
        {ExpectedDelegateInvokeCallText}
    }}
}}";
        private static string ExpectedFullyModifiedMiddlewareClassText =>
$@"{ExpectedMiddlewareClassSignature}
{{
    {ExpectedDelegateFieldDeclarationText}
    {SyntaxHelperSetupFixture.AdditionalFieldText}
    {SyntaxHelperSetupFixture.AdditionalPropertyText}
    {{
        get;
    }}

    {ExpectedMiddlewareConstructorSignature}
    {{
        {ExpectedDelegateConstructorAssignmentText}
        {SyntaxHelperSetupFixture.AdditionalStatementText}
    }}

    {ExpectedMiddlewareInvokeSignature}
    {{
        {SyntaxHelperSetupFixture.AdditionalStatementText}
        {ExpectedDelegateInvokeCallText}
        {SyntaxHelperSetupFixture.AdditionalStatementText}
    }}

    {SyntaxHelperSetupFixture.AdditionalMethodSignature}
    {{
    }}
}}";
        private static string ExpectedBasicMiddlewareConstructorText =>
$@"{ExpectedMiddlewareConstructorSignature}
{{
    {ExpectedDelegateConstructorAssignmentText}
}}";
        private static string ExpectedModifiedMiddlewareConstructorText =>
$@"{ExpectedMiddlewareConstructorSignature}
{{
    {ExpectedDelegateConstructorAssignmentText}
    {SyntaxHelperSetupFixture.AdditionalStatementText}
}}";
        private static string ExpectedBasicInvokeText =>
$@"{ExpectedMiddlewareInvokeSignature}
{{
    {ExpectedDelegateInvokeCallText}
}}";
        private static string ExpectedPreHandledInvokeText =>
$@"{ExpectedMiddlewareInvokeSignature}
{{
    {SyntaxHelperSetupFixture.AdditionalStatementText}
    {ExpectedDelegateInvokeCallText}
}}";
        private static string ExpectedPostHandledInvokeText =>
$@"{ExpectedMiddlewareInvokeSignature}
{{
    {ExpectedDelegateInvokeCallText}
    {SyntaxHelperSetupFixture.AdditionalStatementText}
}}";

        [Test]
        public void BuildMiddlewareClass_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicMiddlewareClassText, MiddlewareSyntaxHelper.BuildMiddlewareClass(MiddlewareClassName).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildMiddlewareClass_Produces_Expected_Fully_Modified_Result()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            var modifiedStartupClass = MiddlewareSyntaxHelper.BuildMiddlewareClass(
                middlewareClassName: MiddlewareClassName,
                constructorAdditionalStatements: additionalStatements,
                preHandleStatements: additionalStatements,
                postHandleStatements: additionalStatements,
                additionalFieldDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalFieldDeclaration },
                additionalPropertyDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalPropertyDeclaration },
                additionalMethodDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalMethodDeclaration });

            Assert.AreEqual(ExpectedFullyModifiedMiddlewareClassText, modifiedStartupClass.NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void AddMiddlewareFields_Produces_Expected_Basic_Result()
        {
            var expectedDeclarationStrings = new[] { ExpectedDelegateFieldDeclarationText };
            var actualDeclarationStrings = MiddlewareSyntaxHelper.AddMiddlewareFields().Select(declaration => declaration.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedDeclarationStrings, actualDeclarationStrings);
        }

        [Test]
        public void AddMiddlewareFields_Produces_Expected_Result_When_Additional_Fields_Present()
        {
            var additionalFieldDeclarations = new[] { SyntaxHelperSetupFixture.AdditionalFieldDeclaration };
            var expectedDeclarationStrings = new[] { ExpectedDelegateFieldDeclarationText, SyntaxHelperSetupFixture.AdditionalFieldText };
            var actualDeclarationStrings = MiddlewareSyntaxHelper.AddMiddlewareFields(additionalFieldDeclarations)
                .Select(declaration => declaration.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedDeclarationStrings, actualDeclarationStrings);
        }

        [Test]
        public void BuildMiddlewareConstructor_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicMiddlewareConstructorText, MiddlewareSyntaxHelper.BuildMiddlewareConstructor(MiddlewareClassName).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildMiddlewareConstructor_Produces_Expected_Result_When_Additonal_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            var actualConstructorText = MiddlewareSyntaxHelper.BuildMiddlewareConstructor(MiddlewareClassName, additionalStatements).NormalizeWhitespace().ToFullString();
            Assert.AreEqual(ExpectedModifiedMiddlewareConstructorText, actualConstructorText);
        }

        [Test]
        public void BuildMiddlewareInvokeMethod_Producers_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicInvokeText, MiddlewareSyntaxHelper.BuildMiddlewareInvokeMethod().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildMiddlewareInvokeMethod_Producers_Expected_Result_When_Pre_Handle_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            var actualInvokeText = MiddlewareSyntaxHelper.BuildMiddlewareInvokeMethod(preHandleStatements: additionalStatements).NormalizeWhitespace().ToFullString();
            Assert.AreEqual(ExpectedPreHandledInvokeText, actualInvokeText);
        }

        [Test]
        public void BuildMiddlewareInvokeMethod_Producers_Expected_Result_When_Post_Handle_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            var actualInvokeText = MiddlewareSyntaxHelper.BuildMiddlewareInvokeMethod(postHandleStatements: additionalStatements).NormalizeWhitespace().ToFullString();
            Assert.AreEqual(ExpectedPostHandledInvokeText, actualInvokeText);
        }

        [Test]
        public void BuildMiddlewareRegistrationSyntax_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedMiddlwareRegistrationString, MiddlewareSyntaxHelper.BuildMiddlewareRegistrationSyntax(MiddlewareClassName).NormalizeWhitespace().ToFullString());
        }
    }
}
