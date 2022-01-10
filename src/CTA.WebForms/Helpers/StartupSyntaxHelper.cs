using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.Helpers
{
    public static class StartupSyntaxHelper
    {
        public const string StartupClassName = "Startup";
        public const string StartupConfigureMethodName = "Configure";
        public const string StartupConfigureServicesMethodName = "ConfigureServices";

        public static IEnumerable<string> RequiredNamespaces => new[]
        {
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Builder",
            "Microsoft.Extensions.Configuration",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Hosting"
        };

        /// <summary>
        /// Condition that will return true when in development
        /// </summary>
        public static string DevEnvConditionText => $"{RuntimeInjectable.EnvInjectable.PropertyName}.IsDevelopment()";

        /// <summary>
        /// Required statement in Configure method, adds use of wwwroot to Blazor app
        /// </summary>
        public static string AppUseStaticFilesText => $"{RuntimeInjectable.AppBuilderInjectable.ParamName}.UseStaticFiles();";
        /// <summary>
        /// Required statement in Configure method, adds routing capabilities to Blazor app
        /// </summary>
        public static string AppUseRoutingText => $"{RuntimeInjectable.AppBuilderInjectable.ParamName}.UseRouting();";
        /// <summary>
        /// Statement that will allow us to debug easier when running resulting project, uses a detailed
        /// dev error page on exception
        /// </summary>
        public static string AppUseDevExceptionPageCall => $"{RuntimeInjectable.AppBuilderInjectable.ParamName}.UseDeveloperExceptionPage();";
        /// <summary>
        /// Statement that will configure router and other stuff required for normal operation of resulting project
        /// </summary>
        public static string AppUseEndpointsCall => $@"{RuntimeInjectable.AppBuilderInjectable.ParamName}.UseEndpoints(endpoints =>
            {{endpoints.MapBlazorHub();endpoints.MapFallbackToPage(""/_Host"");}});";

        /// <summary>
        /// Required statement in ConfigureServices method, adds use of Razor templated pages
        /// </summary>
        public static string ServiceAddRazorPagesText => $"{RuntimeInjectable.ServiceCollectionInjectable.ParamName}.AddRazorPages();";
        /// <summary>
        /// Required statement in ConfigureServices method, configures Blazor app to be Blazor Server
        /// </summary>
        public static string ServiceAddServerSideBlazorText => $"{RuntimeInjectable.ServiceCollectionInjectable.ParamName}.AddServerSideBlazor();";

        /// <summary>
        /// A shortcut method to make building the entire class;
        /// more hassle free than assembling each of the components
        /// individually (which is possible if specialized functionality
        /// is needed, but not recommended)
        /// </summary>
        /// <param name="constructorAdditionalStatements">Statements to put after required statements in constructor</param>
        /// <param name="configureAdditionalStatements">Statements to put after required statements in configure method</param>
        /// <param name="configureServicesAdditionalStatements">Statements to put after required statements in configure services method</param>
        /// <param name="additionalFieldDeclarations">Fields that were used in Global.asax.cs</param>
        /// <param name="additionalPropertyDeclarations">Properties that were used in Global.asax.cs</param>
        /// <param name="additionalMethodDeclarations">Methods in addition to the "normal" methods of Global.asax.cs</param>
        /// <returns>ClassDeclarationSyntax node for new Startup class</returns>
        public static ClassDeclarationSyntax ConstructStartupClass(
            IEnumerable<StatementSyntax> constructorAdditionalStatements = null,
            IEnumerable<StatementSyntax> configureAdditionalStatements = null,
            IEnumerable<StatementSyntax> configureServicesAdditionalStatements = null,
            IEnumerable<FieldDeclarationSyntax> additionalFieldDeclarations = null,
            IEnumerable<PropertyDeclarationSyntax> additionalPropertyDeclarations = null,
            IEnumerable<MethodDeclarationSyntax> additionalMethodDeclarations = null)
        {
            var result = SyntaxFactory.ClassDeclaration(StartupClassName).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (additionalFieldDeclarations != null)
            {
                result = result.AddMembers(additionalFieldDeclarations.ToArray());
            }

            result = result.AddMembers(AddStartupProperties(additionalPropertyDeclarations).ToArray());

            result = result.AddMembers(
                    ConstructStartupConstructor(constructorAdditionalStatements),
                    ConstructStartupConfigureMethod(configureAdditionalStatements),
                    ConstructStartupConfigureServicesMethod(configureServicesAdditionalStatements));

            if (additionalMethodDeclarations != null)
            {
                result = result.AddMembers(additionalMethodDeclarations.ToArray());
            }

            return result;
        }

        public static IEnumerable<PropertyDeclarationSyntax> AddStartupProperties(IEnumerable<PropertyDeclarationSyntax> additionalDeclarations = null)
        {
            var newProperties = new List<PropertyDeclarationSyntax>()
            {
                // We guarantee that config and env will be present as properties
                RuntimeInjectable.ConfigInjectable.GetPropertyDeclaration(),
                RuntimeInjectable.EnvInjectable.GetPropertyDeclaration()
            };

            if (additionalDeclarations == null)
            {
                return newProperties;
            }

            return newProperties.UnionSyntaxNodeCollections(additionalDeclarations);
        }

        public static ConstructorDeclarationSyntax ConstructStartupConstructor(IEnumerable<StatementSyntax> additonalStatements = null)
        {
            var statements = new List<StatementSyntax>()
            {
                // We guarantee that config and env will be assigned values from
                // constructor params
                RuntimeInjectable.ConfigInjectable.PropertyAssignment,
                RuntimeInjectable.EnvInjectable.PropertyAssignment
            };

            if (additonalStatements != null)
            {
                statements.AddRange(additonalStatements);
            }

            return SyntaxFactory.ConstructorDeclaration(StartupClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                // Accept runtime injected config and env services
                .AddParameterListParameters(RuntimeInjectable.ConfigInjectable.AsParameter, RuntimeInjectable.EnvInjectable.AsParameter)
                .WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));
        }

        public static MethodDeclarationSyntax ConstructStartupConfigureMethod(IEnumerable<StatementSyntax> additonalStatements = null)
        {
            var devOnlyStatements = new List<StatementSyntax>()
            {
                SyntaxFactory.ParseStatement(AppUseDevExceptionPageCall)
            };

            var devEnvCondition = SyntaxFactory.ParseExpression(DevEnvConditionText);

            var statements = new List<StatementSyntax>()
            {
                // Standard blazor configuration statements
                SyntaxFactory.ParseStatement(AppUseStaticFilesText),
                SyntaxFactory.ParseStatement(AppUseRoutingText),
                SyntaxFactory.ParseStatement(AppUseEndpointsCall),
                // Dev env dependent statements
                SyntaxFactory.IfStatement(devEnvCondition, CodeSyntaxHelper.GetStatementsAsBlock(devOnlyStatements))
            };

            if (additonalStatements != null)
            {
                statements.AddRange(additonalStatements);
            }

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), StartupConfigureMethodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                // Accept runtime injected application builder
                .AddParameterListParameters(RuntimeInjectable.AppBuilderInjectable.AsParameter)
                .WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));
        }

        public static MethodDeclarationSyntax ConstructStartupConfigureServicesMethod(IEnumerable<StatementSyntax> additonalStatements = null)
        {
            var statements = new List<StatementSyntax>()
            {
                // Standard blazor configuration statements
                SyntaxFactory.ParseStatement(ServiceAddRazorPagesText),
                SyntaxFactory.ParseStatement(ServiceAddServerSideBlazorText)
            };


            if (additonalStatements != null)
            {
                statements.AddRange(additonalStatements);
            }

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), StartupConfigureServicesMethodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                // Accept runtime injected service collection
                .AddParameterListParameters(RuntimeInjectable.ServiceCollectionInjectable.AsParameter)
                .WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));
        }
    }
}
