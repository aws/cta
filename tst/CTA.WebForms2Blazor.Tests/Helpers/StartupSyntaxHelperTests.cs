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

        private const string ExpectedConfigPropertyDeclaration = "public IConfiguration Configuration";
        private const string ExpectedEnvPropertyDeclaration = "public IWebHostEnvironment Env";

        private const string ExpectedConfigureUseStaticFilesCall = "app.UseStaticFiles();";
        private const string ExpectedConfigureUseRoutingCall = "app.UseRouting();";
        private const string ExpectedDevErrorPageCall = "app.UseDeveloperExceptionPage();";

        private const string ExpectedConfigureServicesAddRazorPagesCall = "services.AddRazorPages();";
        private const string ExpectedConfigureServicesAddServerSideBlazorCall = "services.AddServerSideBlazor();";

        private static string ExpectedConfigPropertyDeclarationFullText =>
$@"{ExpectedConfigPropertyDeclaration}
{{
    get;
}}";
        private static string ExpectedEnvPropertyDeclarationFullText =>
$@"{ExpectedEnvPropertyDeclaration}
{{
    get;
}}";

        private static string ExpectedBasicStartupClassText =>
$@"{ExpectedStartupClassSignature}
{{
    {ExpectedConfigPropertyDeclaration}
    {{
        get;
    }}

    {ExpectedEnvPropertyDeclaration}
    {{
        get;
    }}

    {ExpectedBasicStartupConstructorSignature}
    {{
        {ExpectedConstructorConfigAssignment}
        {ExpectedConstructorEnvAssignment}
    }}

    {ExpectedBasicConfigureMethodSignature}
    {{
        {ExpectedConfigureUseStaticFilesCall}
        {ExpectedConfigureUseRoutingCall}
        app.UseEndpoints(endpoints =>
        {{
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage(""/_Host"");
        }});
        if (Env.IsDevelopment())
        {{
            {ExpectedDevErrorPageCall}
        }}
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
    {{
        get;
    }}

    {ExpectedEnvPropertyDeclaration}
    {{
        get;
    }}

    {SyntaxHelperSetupFixture.AdditionalPropertyText}
    {{
        get;
    }}

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
        app.UseEndpoints(endpoints =>
        {{
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage(""/_Host"");
        }});
        if (Env.IsDevelopment())
        {{
            {ExpectedDevErrorPageCall}
        }}

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
    app.UseEndpoints(endpoints =>
    {{
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage(""/_Host"");
    }});
    if (Env.IsDevelopment())
    {{
        {ExpectedDevErrorPageCall}
    }}
}}";
        private static string ExpectedModifiedConfigureText =>
$@"{ExpectedBasicConfigureMethodSignature}
{{
    {ExpectedConfigureUseStaticFilesCall}
    {ExpectedConfigureUseRoutingCall}
    app.UseEndpoints(endpoints =>
    {{
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage(""/_Host"");
    }});
    if (Env.IsDevelopment())
    {{
        {ExpectedDevErrorPageCall}
    }}

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
        public void ConstructStartupClass_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicStartupClassText, StartupSyntaxHelper.ConstructStartupClass().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void ConstructStartupClass_Produces_Expected_Fully_Modified_Result()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            var modifiedStartupClass = StartupSyntaxHelper.ConstructStartupClass(
                constructorAdditionalStatements: additionalStatements,
                configureAdditionalStatements: additionalStatements,
                configureServicesAdditionalStatements: additionalStatements,
                additionalFieldDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalFieldDeclaration },
                additionalPropertyDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalPropertyDeclaration },
                additionalMethodDeclarations: new[] { SyntaxHelperSetupFixture.AdditionalMethodDeclaration });

            Assert.AreEqual(ExpectedFullyModifiedClassText, modifiedStartupClass.NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void AddStartupProperties_Produces_Expected_Basic_Result()
        {
            var expectedDeclarationStrings = new[] { ExpectedConfigPropertyDeclarationFullText, ExpectedEnvPropertyDeclarationFullText };
            var actualDeclarationStrings = StartupSyntaxHelper.AddStartupProperties().Select(declaration => declaration.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedDeclarationStrings, actualDeclarationStrings);
        }

        [Test]
        public void AddStartupProperties_Produces_Expected_Result_When_Additional_Properties_Present()
        {
            var additionalPropertyDeclarations = new[] { SyntaxHelperSetupFixture.AdditionalPropertyDeclaration };
            var expectedDeclarationStrings = new[]
            {
                ExpectedConfigPropertyDeclarationFullText,
                ExpectedEnvPropertyDeclarationFullText,
                SyntaxHelperSetupFixture.AdditionalPropertyFullText
            };
            var actualDeclarationStrings = StartupSyntaxHelper.AddStartupProperties(additionalPropertyDeclarations)
                .Select(declaration => declaration.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedDeclarationStrings, actualDeclarationStrings);
        }

        [Test]
        public void ConstructStartupConstructor_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicStartupConstructorText, StartupSyntaxHelper.ConstructStartupConstructor().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void ConstructStartupConstructor_Produces_Expected_Result_When_Additional_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            Assert.AreEqual(ExpectedModifiedStartupConstructorText, StartupSyntaxHelper.ConstructStartupConstructor(additionalStatements).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void ConstructStartupConfigureMethod_Produces_Expected_Basic_Result()
        {
            Assert.AreEqual(ExpectedBasicConfigureText, StartupSyntaxHelper.ConstructStartupConfigureMethod().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void ConstructStartupConfigureMethod_Produces_Expected_Result_When_Additional_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            Assert.AreEqual(ExpectedModifiedConfigureText, StartupSyntaxHelper.ConstructStartupConfigureMethod(additionalStatements).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void ConstructStartupConfigureServicesMethod_Produces_Expected_Result()
        {
            Assert.AreEqual(ExpectedBasicConfigureServicesText, StartupSyntaxHelper.ConstructStartupConfigureServicesMethod().NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void ConstructStartupConfigureServicesMethod_Produces_Expected_Result_When_Additional_Statements_Present()
        {
            var additionalStatements = new[] { SyntaxHelperSetupFixture.AdditionalStatement };
            Assert.AreEqual(ExpectedModifiedConfigureServicesText, StartupSyntaxHelper.ConstructStartupConfigureServicesMethod(additionalStatements).NormalizeWhitespace().ToFullString());
        }
    }
}
