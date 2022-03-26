using CTA.WebForms.ClassConverters;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms.Metrics;

namespace CTA.WebForms.Tests.ClassConverters
{
    public class PageCodeBehindClassConverterTests
    {
        private const string InputComplexClassText =
@"namespace TestNamespace
{
    public class TestPageClass
    {
        private int thousands = 2000;
        public bool MyBoolProperty { get; set; }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            TestMethod1();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            char c1 = 'a';
            char c2 = 'b';
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            thousands *= 1000;
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            TestMethod1();
        }

        public void TestMethod1()
        {
            MyBoolProperty = true;
        }
    }
}";
        private const string ExpectedOutputComplexClassText =
@"using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class TestPageClass : ComponentBase, IDisposable
    {
        private int thousands = 2000;
        public bool MyBoolProperty { get; set; }

        public void TestMethod1()
        {
            MyBoolProperty = true;
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            // This code replaces the original handling
            // of the PreInit event
            TestMethod1();
            
            // This code replaces the original handling
            // of the Init event
            char c1 = 'a';
            char c2 = 'b';
            
            await base.SetParametersAsync(parameters);
        }

        protected override void OnInitialized()
        {
            // This code replaces the original handling
            // of the Load event
            thousands *= 1000;
        }

        public void Dispose()
        {
            // This code replaces the original handling
            // of the Unload event
            TestMethod1();
        }
    }
}";

        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "CodeBehind.aspx.cs");
        private static string ExpectedOutputPath => Path.Combine("Pages", ClassConverterSetupFixture.TestProjectNestedDirectoryName, "CodeBehind.razor.cs");

        private PageCodeBehindClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new PageCodeBehindClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol,
                new TaskManagerService(),
                new CodeBehindReferenceLinkerService(),
                new WebFormMetricContext());
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = (await _converter.MigrateClassAsync()).Single();

            Assert.AreEqual(ExpectedOutputPath, fileInfo.RelativePath);
        }

        [Test]
        public async Task MigrateClassAsync_Correctly_Builds_Complex_Page_Component_Class()
        {
            var complexSyntaxTree = SyntaxFactory.ParseSyntaxTree(InputComplexClassText);
            var complexSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { complexSyntaxTree }).GetSemanticModel(complexSyntaxTree);
            var complexClassDec = complexSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var complexTypeSymbol = complexSemanticModel.GetDeclaredSymbol(complexClassDec);

            var complexConverter = new PageCodeBehindClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                complexSemanticModel,
                complexClassDec,
                complexTypeSymbol,
                new TaskManagerService(),
                new CodeBehindReferenceLinkerService(),
                new WebFormMetricContext());

            var fileInfo = (await complexConverter.MigrateClassAsync()).Single();
            var fileText = Encoding.UTF8.GetString(fileInfo.FileBytes);

            Assert.AreEqual(ExpectedOutputComplexClassText, fileText);
        }
    }
}
