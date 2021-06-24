using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    [SetUpFixture]
    public class SyntaxHelperSetupFixture
    {
        public const string AdditionalMethodName = "TestMethod";
        public const string AdditionalPropertyName = "TestProperty";
        public const string AdditionalFieldName = "_testField";
        public const string AdditionalStatementText = "var x = 10;";

        public static string AdditionalMethodSignature => $"void {AdditionalMethodName}()";
        public static string AdditionalPropertyText => $"public int {AdditionalPropertyName}";
        public static string AdditionalFieldText => $"private int {AdditionalFieldName};";
        public static string AdditionalPropertyFullText =>
$@"public int {AdditionalPropertyName}
{{
    get;
}}";

        public static MethodDeclarationSyntax AdditionalMethodDeclaration => SyntaxFactory
            .MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), AdditionalMethodName)
            .WithBody(SyntaxFactory.Block());
        public static PropertyDeclarationSyntax AdditionalPropertyDeclaration => SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)), AdditionalPropertyName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(SyntaxFactory
                .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        public static FieldDeclarationSyntax AdditionalFieldDeclaration => SyntaxFactory
            .FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))))
            .AddDeclarationVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(AdditionalFieldName)))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

        public static StatementSyntax AdditionalStatement => SyntaxFactory.ParseStatement(AdditionalStatementText);
    }
}
