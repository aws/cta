using System.Collections.Generic;
using System.Linq;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Helpers
{
    public static class StartupSyntaxHelper
    {
        public const string StartupClassName = "Startup";
        public const string StartupConfigureMethodName = "Configure";
        public const string StartupConfigureServicesMethodName = "ConfigureServices";

        // Required statements in Configure method
        public static string AppUseStaticFilesText => $"{RuntimeInjectable.AppBuilderInjectable.ParamName}.UseStaticFiles();";
        public static string AppUseRoutingText => $"{RuntimeInjectable.AppBuilderInjectable.ParamName}.UseRouting();";

        // Required statements in ConfigureServices method
        public static string ServiceAddRazorPagesText => $"{RuntimeInjectable.ServiceCollectionInjectable.ParamName}.AddRazorPages();";
        public static string ServiceAddServerSideBlazorText => $"{RuntimeInjectable.ServiceCollectionInjectable.ParamName}.AddServerSideBlazor();";

        // A shortcut method to make building the entire class;
        // more hassle free than assembling each of the components
        // individually (which is possible if specialized functionality
        // is needed, but not recommended)
        public static ClassDeclarationSyntax BuildStartupClass(
            // Statements to put after required statements in constructor
            IEnumerable<StatementSyntax> constructorAdditionalStatements = null,
            // Statements to put after required statements in configure method
            IEnumerable<StatementSyntax> configureAdditionalStatements = null,
            // Statements to put after required statements in configure services method
            IEnumerable<StatementSyntax> configureServicesAdditionalStatements = null,
            // Fields that were used in Global.asax.cs
            IEnumerable<FieldDeclarationSyntax> fieldDeclarations = null,
            // Properties that were used in Global.asax.cs
            IEnumerable<PropertyDeclarationSyntax> additionalPropertyDeclarations = null,
            // Methods in addition to the "normal" methods of Global.asax.cs
            IEnumerable<MethodDeclarationSyntax> additionalMethodDeclarations = null)
        {
            var result = SyntaxFactory.ClassDeclaration(StartupClassName).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (fieldDeclarations != null)
            {
                result = result.AddMembers(fieldDeclarations.ToArray());
            }

            result = result.AddMembers(AddStartupProperties(additionalPropertyDeclarations).ToArray());

            result = result.AddMembers(
                    BuildStartupConstructor(constructorAdditionalStatements),
                    BuildStartupConfigureMethod(configureAdditionalStatements),
                    BuildStartupConfigureServicesMethod(configureServicesAdditionalStatements));

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

        public static ConstructorDeclarationSyntax BuildStartupConstructor(IEnumerable<StatementSyntax> additonalStatements = null)
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

        public static MethodDeclarationSyntax BuildStartupConfigureMethod(IEnumerable<StatementSyntax> additonalStatements = null)
        {
            var statements = new List<StatementSyntax>()
            {
                // Standard blazor configuration statements
                SyntaxFactory.ParseStatement(AppUseStaticFilesText),
                SyntaxFactory.ParseStatement(AppUseRoutingText)
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

        public static MethodDeclarationSyntax BuildStartupConfigureServicesMethod(IEnumerable<StatementSyntax> additonalStatements = null)
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
