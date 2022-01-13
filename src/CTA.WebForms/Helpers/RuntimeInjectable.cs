using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.Helpers
{
    public class RuntimeInjectable
    {
        private const string InjectableNameArgumentError = "InjectableName must have at least 1 character";

        // These structures make basic syntax tree component
        // building easier (and more easily interchangeable)
        // for runtime injectable types we know may be used in
        // startup and middleware
        public static RuntimeInjectable ConfigInjectable => new RuntimeInjectable("Configuration", "IConfiguration");
        public static RuntimeInjectable EnvInjectable => new RuntimeInjectable("Env", "IWebHostEnvironment");
        public static RuntimeInjectable ServiceCollectionInjectable => new RuntimeInjectable("Services", "IServiceCollection");
        public static RuntimeInjectable AppBuilderInjectable => new RuntimeInjectable("App", "IApplicationBuilder");
        public static RuntimeInjectable RequestDelegateInjectable => new RuntimeInjectable("Next", "RequestDelegate");
        public static RuntimeInjectable HttpContextInjectable => new RuntimeInjectable("Context", "HttpContext");
        public static RuntimeInjectable ParameterViewInjectable => new RuntimeInjectable("Parameters", "ParameterView");

        private readonly string _nameFirstChar;
        private readonly string _nameRestOfChars;
        private readonly string _typeName;

        public string ParamName { get { return _nameFirstChar.ToLower() + _nameRestOfChars; } }
        public string PropertyName { get { return _nameFirstChar.ToUpper() + _nameRestOfChars; } }
        public string PublicFieldName { get { return ParamName; } }
        public string PrivateFieldName { get { return $"_{ParamName}"; } }
        public ParameterSyntax AsParameter => SyntaxFactory.Parameter(SyntaxFactory.Identifier(ParamName)).WithType(SyntaxFactory.ParseTypeName(_typeName));
        public StatementSyntax PropertyAssignment => SyntaxFactory.ParseStatement($"{PropertyName} = {ParamName};");
        public StatementSyntax PublicFieldAssignment => SyntaxFactory.ParseStatement($"{PublicFieldName} = {ParamName};");
        public StatementSyntax PrivateFieldAssignment => SyntaxFactory.ParseStatement($"{PrivateFieldName} = {ParamName};");

        private RuntimeInjectable(string injectableName, string injectableTypeName)
        {
            if (string.IsNullOrEmpty(injectableName))
            {
                throw new ArgumentException(InjectableNameArgumentError);
            }

            _nameFirstChar = injectableName.Substring(0, 1);
            // Substring returns string.Empty if length is 1, this
            // is ok for our purposes
            _nameRestOfChars = injectableName.Substring(1);
            _typeName = injectableTypeName;
        }

        public PropertyDeclarationSyntax GetPropertyDeclaration(bool allowGet = true, bool allowSet = false)
        {
            var declaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(_typeName), PropertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (allowGet)
            {
                declaration = declaration.AddAccessorListAccessors(SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }

            if (allowSet)
            {
                declaration = declaration.AddAccessorListAccessors(SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }

            return declaration;
        }

        public FieldDeclarationSyntax GetFieldDeclaration(bool isPrivate = true, bool isReadOnly = true)
        {
            var declaration = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(_typeName)));

            if (isPrivate)
            {
                declaration = declaration.AddDeclarationVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(PrivateFieldName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            }
            else
            {
                declaration = declaration.AddDeclarationVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(PublicFieldName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            }

            if (isReadOnly)
            {
                declaration = declaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
            }

            return declaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
