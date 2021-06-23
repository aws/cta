using CTA.WebForms2Blazor.Helpers;
using NUnit.Framework;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    [TestFixture]
    public class RuntimeInjectableTests
    {
        private const string TestInjectableNamePascalCase = "TestName";
        private const string TestInjectableNameCamelCase = "testName";
        private const string TestInjectableNameCamelCaseUnderscore = "_testName";
        private const string TestInjectableTypeName = "TestTypeName";
        private static string TestInjectablePropertyGetter => $"public {TestInjectableTypeName} {TestInjectableNamePascalCase} {{ get; }}";
        private static string TestInjectablePropertySetter => $"public {TestInjectableTypeName} {TestInjectableNamePascalCase} {{ set; }}";
        private static string TestInjectablePropertyGetterSetter => $"public {TestInjectableTypeName} {TestInjectableNamePascalCase} {{ get; set; }}";
        private static string TestInjectablePrivateField => $"private {TestInjectableTypeName} {TestInjectableNameCamelCaseUnderscore};";
        private static string TestInjectablePublicField => $"public {TestInjectableTypeName} {TestInjectableNameCamelCase};";
        private static string TestInjectablePublicReadonlyField => $"public readonly {TestInjectableTypeName} {TestInjectableNameCamelCase};";

        private RuntimeInjectable _testRuntimeInjectable;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testRuntimeInjectable = new RuntimeInjectable(TestInjectableNamePascalCase, TestInjectableTypeName);
        }

        [Test]
        public void ParamName_Is_Camel_Case()
        {
            Assert.AreEqual(TestInjectableNameCamelCase, _testRuntimeInjectable.ParamName);
        }

        [Test]
        public void PropertyName_Is_Pascal_Case()
        {
            Assert.AreEqual(TestInjectableNamePascalCase, _testRuntimeInjectable.PropertyName);
        }
            
        [Test]
        public void PublicFieldName_Is_Camel_Case()
        {
            Assert.AreEqual(TestInjectableNameCamelCase, _testRuntimeInjectable.PublicFieldName);
        }

        [Test]
        public void PrivateFieldName_Is_Camel_Case_With_Underscore()
        {
            Assert.AreEqual(TestInjectableNameCamelCaseUnderscore, _testRuntimeInjectable.PrivateFieldName);
        }

        [Test]
        public void GetPropertyDeclaration_Correctly_Builds_Getter()
        {
            Assert.AreEqual(TestInjectablePropertyGetter, _testRuntimeInjectable.GetPropertyDeclaration(true, false).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void GetPropertyDeclaration_Correctly_Builds_Setter()
        {
            Assert.AreEqual(TestInjectablePropertySetter, _testRuntimeInjectable.GetPropertyDeclaration(false, true).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void GetPropertyDeclaration_Correctly_Builds_Getter_And_Setter_Together()
        {
            Assert.AreEqual(TestInjectablePropertyGetterSetter, _testRuntimeInjectable.GetPropertyDeclaration(true, true).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void GetFieldDeclaration_Correctly_Builds_Private_Field()
        {
            Assert.AreEqual(TestInjectablePrivateField, _testRuntimeInjectable.GetFieldDeclaration(true, false).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void GetFieldDeclaration_Correctly_Builds_Public_Field()
        {
            Assert.AreEqual(TestInjectablePublicField, _testRuntimeInjectable.GetFieldDeclaration(false, false).NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void GetFieldDeclaration_Correctly_Builds_Readonly_Field()
        {
            Assert.AreEqual(TestInjectablePublicReadonlyField, _testRuntimeInjectable.GetFieldDeclaration(false, true).NormalizeWhitespace().ToFullString());
        }
    }
}
