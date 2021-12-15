using CTA.WebForms2Blazor.Helpers;
using NUnit.Framework;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    public class CodeSyntaxHelperTests
    {
        private const string TestReferencedNamespace1 = "Referenced.Namespace.One";
        private const string TestReferencedNamespace2 = "Referenced.Namespace.Two";
        private const string TestStatement1 = "var x = 10;";
        private const string TestStatement2 = "var y = x + x";
        private const string TestNamespaceName = "TestNamespace";
        private const string TestNullNamespaceName = "Empty_Namespace";
        private const string TestClassName = "TestClass";

        private static string ExpectedTestReferencedNamespaceUsing1 => $"using {TestReferencedNamespace1};";
        private static string ExpectedTestReferencedNamespaceUsing2 => $"using {TestReferencedNamespace2};";
        private static string ExpectedTestNamespaceText =>
$@"namespace {TestNamespaceName}
{{
    class {TestClassName}
    {{
    }}
}}";
        private static string ExpectedTestNullNamespaceText =>
$@"namespace {TestNullNamespaceName}
{{
    class {TestClassName}
    {{
    }}
}}";
        private static string ExpectedBlockText =>
$@"{{
    {TestStatement1}
    {TestStatement2}
}}";
        private static string ExpectedTestFileText =>
$@"{ExpectedTestReferencedNamespaceUsing1}
{ExpectedTestReferencedNamespaceUsing2}

{ExpectedTestNamespaceText}";

        [Test]
        public void BuildUsingStatement_Correctly_Builds_Single_Using_Statement()
        {
            Assert.AreEqual(ExpectedTestReferencedNamespaceUsing1, CodeSyntaxHelper.BuildUsingStatement(TestReferencedNamespace1).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void BuildUsingStatements_Correctly_Builds_Many_Using_Statements()
        {
            var expectedCollection = new[] {
                ExpectedTestReferencedNamespaceUsing1,
                ExpectedTestReferencedNamespaceUsing2
            };

            var actualCollection = CodeSyntaxHelper
                .BuildUsingStatements(new[] { TestReferencedNamespace1, TestReferencedNamespace2 })
                .Select(statement => statement.NormalizeWhitespace().ToFullString());

            Assert.AreEqual(expectedCollection, actualCollection);
        }

        [Test]
        public void BuildNamespace_Correctly_Builds_Namespace_With_Contained_Type()
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(TestClassName);

            var actualNamespaceTexrt = CodeSyntaxHelper.BuildNamespace(null, classDeclaration).NormalizeWhitespace().ToFullString();
            var actualNamespaceText = CodeSyntaxHelper.BuildNamespace(TestNamespaceName, classDeclaration).NormalizeWhitespace().ToFullString();

            Assert.AreEqual(ExpectedTestNamespaceText, actualNamespaceText);
        }

        [Test]
        public void BuildNamespace_Correctly_Builds_Namespace_With_Null_NamespaceName()
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(TestClassName);
            string nullNamespaceName = null;
            var actualNamespaceText = CodeSyntaxHelper.BuildNamespace(nullNamespaceName, classDeclaration).NormalizeWhitespace().ToFullString();
            
            Assert.AreEqual(ExpectedTestNullNamespaceText, actualNamespaceText);
        }

        [Test]
        public void GetStatementsAsBlock_Inserts_Statements_Into_Braced_Code_Block()
        {
            var actualCodeBlockText = CodeSyntaxHelper.GetStatementsAsBlock(new[] {
                SyntaxFactory.ParseStatement(TestStatement1), SyntaxFactory.ParseStatement(TestStatement2)
            }).NormalizeWhitespace().ToFullString();

            Assert.AreEqual(ExpectedBlockText, actualCodeBlockText);
        }

        [Test]
        public void GetFileSyntaxAsString_Properly_Namespace_And_Usings_For_Full_File_Text()
        {
            var usingsCollection = CodeSyntaxHelper.BuildUsingStatements(new[] { TestReferencedNamespace1, TestReferencedNamespace2 });
            var classDeclaration = SyntaxFactory.ClassDeclaration(TestClassName);
            var namespaceDeclaration = CodeSyntaxHelper.BuildNamespace(TestNamespaceName, classDeclaration);

            Assert.AreEqual(ExpectedTestFileText, CodeSyntaxHelper.GetFileSyntaxAsString(namespaceDeclaration, usingsCollection));
        }
    }
}
