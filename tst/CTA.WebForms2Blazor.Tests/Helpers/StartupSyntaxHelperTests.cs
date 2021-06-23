using CTA.WebForms2Blazor.Helpers;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    public class StartupSyntaxHelperTests
    {
        private const string ExpectedStartupClassSignature = "public class Startup";
        private const string ExpectedBasicStartupConstructorSignature = "public Startup(IConfiguration configuration, IWebHostEnvironment env)";
        private const string ExpectedBasicConfigureMethodSignature = "public void Configure(IApplicationBuilder app)";
        private const string ExpectedBasicConfigureServicesMethodSignature = "public void ConfigureServices(IServiceCollection services)";

        private const string ExpectedConstructorConfigAssignment = "Configuration = configuration;";
        private const string ExpectedConstructorEnvAssignment = "Env = env;";

        private const string ExpectedConfigPropertyDeclaration = "public IConfiguration Configuration { get; }";
        private const string ExpectedEnvPropertyDeclaration = "public IWebHostEnvironment Env { get; }";

        private const string ExpectedConfigureUseStaticFilesCall = "app.UseStaticFiles();";
        private const string ExpectedConfigureUseRoutingCall = "app.UseRouting();";

        private const string ExpectedConfigureServicesAddRazorPagesCall = "services.AddRazorPages();";
        private const string ExpectedConfigureServicesAddServerSideBlazorCall = "services.AddServerSideBlazor();";

        private static string ExpectedBasicStartupClassText =>
$@"{ExpectedStartupClassSignature}
{{
    {ExpectedConfigPropertyDeclaration}

    {ExpectedEnvPropertyDeclaration}

    {ExpectedBasicStartupConstructorSignature}
    {{
        {ExpectedConstructorConfigAssignment}
        {ExpectedConstructorEnvAssignment}
    }}

    {ExpectedBasicConfigureMethodSignature}
    {{
        {ExpectedConfigureUseStaticFilesCall}
        {ExpectedConfigureUseRoutingCall}
    }}

    {ExpectedBasicConfigureServicesMethodSignature}
    {{
        {ExpectedConfigureServicesAddRazorPagesCall}
        {ExpectedConfigureServicesAddServerSideBlazorCall}
    }}
}}";
        private static string ExpectedFullyModifiedClassText =>
$@"{ExpectedStartupClassSignature}
{{
    {SyntaxHelperSetupFixture.AdditionalFieldText}
    {ExpectedConfigPropertyDeclaration}

    {ExpectedEnvPropertyDeclaration}

    {SyntaxHelperSetupFixture.AdditionalPropertyText}

    {ExpectedBasicStartupConstructorSignature}
    {{
        {ExpectedConstructorConfigAssignment}
        {ExpectedConstructorEnvAssignment}
        {SyntaxHelperSetupFixture.AdditionalStatementText}
    }}

    {ExpectedBasicConfigureMethodSignature}
    {{
        {ExpectedConfigureUseStaticFilesCall}
        {ExpectedConfigureUseRoutingCall}
        {SyntaxHelperSetupFixture.AdditionalStatementText}
    }}

    {ExpectedBasicConfigureServicesMethodSignature}
    {{
        {ExpectedConfigureServicesAddRazorPagesCall}
        {ExpectedConfigureServicesAddServerSideBlazorCall}
        {SyntaxHelperSetupFixture.AdditionalStatementText}
    }}

    {SyntaxHelperSetupFixture.AdditionalMethodSignature}
    {{
    }}
}}";
        private static string ExpectedBasicStartupConstructorText =>
$@"{ExpectedBasicStartupConstructorSignature}
{{
    {ExpectedConstructorConfigAssignment}
    {ExpectedConstructorEnvAssignment}
}}";
        private static string ExpectedModifiedStartupConstructorText =>
$@"{ExpectedBasicStartupConstructorSignature}
{{
    {ExpectedConstructorConfigAssignment}
    {ExpectedConstructorEnvAssignment}
    {SyntaxHelperSetupFixture.AdditionalStatementText}
}}";
        private static string ExpectedBasicConfigureText =>
$@"{ExpectedBasicConfigureMethodSignature}
{{
    {ExpectedConfigureUseStaticFilesCall}
    {ExpectedConfigureUseRoutingCall}
}}";
        private static string ExpectedModifiedConfigureText =>
$@"{ExpectedBasicConfigureMethodSignature}
{{
    {ExpectedConfigureUseStaticFilesCall}
    {ExpectedConfigureUseRoutingCall}
    {SyntaxHelperSetupFixture.AdditionalStatementText}
}}";
        private static string ExpectedBasicConfigureServicesText =>
$@"{ExpectedBasicConfigureServicesMethodSignature}
{{
    {ExpectedConfigureServicesAddRazorPagesCall}
    {ExpectedConfigureServicesAddServerSideBlazorCall}
}}";
        private static string ExpectedModifiedConfigureServicesText =>
$@"{ExpectedBasicConfigureServicesMethodSignature}
{{
    {ExpectedConfigureServicesAddRazorPagesCall}
    {ExpectedConfigureServicesAddServerSideBlazorCall}
    {SyntaxHelperSetupFixture.AdditionalStatementText}
}}";

        [Test]
        public void BuildStartupClass_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicStartupClassText, StartupSyntaxHelper.BuildStartupClass().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildStartupClass_Produces_Expected_Fully_Modified_Result()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            var modifiedStartupClass = StartupSyntaxHelper.BuildStartupClass(
                constructorAdditionalStatements: additionalStatements,
                configureAdditionalStatements: additionalStatements,
                configureServicesAdditionalStatements: additionalStatements,
                fieldDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalFieldDeclaration },
                additionalPropertyDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalPropertyDeclaration },
                additionalMethodDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalMethodDeclaration });

            Assert.AreEqual(ExpectedFullyModifiedClassText, modifiedStartupClass.NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void AddStartupProperties_Produces_Expected_Basic_Result()
        {
            var expectedDeclarationStrings = new[] { ExpectedConfigPropertyDeclaration, ExpectedEnvPropertyDeclaration };
            var actualDeclarationStrings = StartupSyntaxHelper.AddStartupProperties().Select(declaration => declaration.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedDeclarationStrings, actualDeclarationStrings);
        }

        [Test]
        public void AddStartupProperties_Produces_Expected_Result_When_Additional_Properties_Present()
        {
            var additionalPropertyDeclarations = new[] { SyntaxHelperSetupFixture.AdditionalPropertyDeclaration };
            var expectedDeclarationStrings = new[] { ExpectedConfigPropertyDeclaration, ExpectedEnvPropertyDeclaration, SyntaxHelperSetupFixture.AdditionalPropertyText };
            var actualDeclarationStrings = StartupSyntaxHelper.AddStartupProperties(additionalPropertyDeclarations)
                .Select(declaration => declaration.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedDeclarationStrings, actualDeclarationStrings);
        }

        [Test]
        public void BuildStartupConstructor_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicStartupConstructorText, StartupSyntaxHelper.BuildStartupConstructor().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildStartupConstructor_Produces_Expected_Result_When_Additional_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            Assert.AreEqual(ExpectedModifiedStartupConstructorText, StartupSyntaxHelper.BuildStartupConstructor(additionalStatements).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildStartupConfigureMethod_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicConfigureText, StartupSyntaxHelper.BuildStartupConfigureMethod().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildStartupConfigureMethod_Produces_Expected_Result_When_Additional_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            Assert.AreEqual(ExpectedModifiedConfigureText, StartupSyntaxHelper.BuildStartupConfigureMethod(additionalStatements).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildStartupConfigureServicesMethod_Produces_Expected_Result()
        {
            Assert.AreEqual(ExpectedBasicConfigureServicesText, StartupSyntaxHelper.BuildStartupConfigureServicesMethod().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildStartupConfigureServicesMethod_Produces_Expected_Result_When_Additional_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            Assert.AreEqual(ExpectedModifiedConfigureServicesText, StartupSyntaxHelper.BuildStartupConfigureServicesMethod(additionalStatements).NormalizeWhitespace().ToFullString());
        }
    }
}
