using CTA.WebForms2Blazor.Tests.ProjectManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    [SetUpFixture]
    public class ClassConverterSetupFixture
    {
        public const string TestProjectDirectoryName = "TestProjectDir";
        public const string TestProjectNestedDirectoryName = "NestedDir1";
        public const string TestClassName = "TestClass1";
        public const string TestNamespaceName = "TestNamespace1";

        public static string TestProjectDirectoryPath;
        public static string TestProjectNestedDirectoryPath;
        public static ClassDeclarationSyntax TestClassDec;
        public static SyntaxTree TestSyntaxTree;
        public static SemanticModel TestSemanticModel;
        public static INamedTypeSymbol TestTypeSymbol;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestProjectDirectoryPath = Path.Combine(PartialProjectSetupFixture.TestFilesPath, TestProjectDirectoryName);
            TestProjectNestedDirectoryPath = Path.Combine(TestProjectDirectoryPath, TestProjectNestedDirectoryName);

            TestClassDec = SyntaxFactory.ClassDeclaration(TestClassName).AddMembers(SyntaxFactory.ConstructorDeclaration(TestClassName));
            TestSyntaxTree = SyntaxFactory.CompilationUnit()
                .AddMembers(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(TestNamespaceName)).AddMembers(TestClassDec)).SyntaxTree;
            TestSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { TestSyntaxTree }).GetSemanticModel(TestSyntaxTree);
            // Fetch updated class dec node
            TestClassDec = TestSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            TestTypeSymbol = TestSemanticModel.GetDeclaredSymbol(TestClassDec);
        }
    }
}
