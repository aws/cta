using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms2Blazor.Metrics;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class ControlCodeBehindClassConverterTests
    {
        private const string ExpectedBaseClass = "ComponentBase";
        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "CodeBehind.ascx.cs");
        private static string ExpectedOutputPath => Path.Combine("Components", ClassConverterSetupFixture.TestProjectNestedDirectoryName, "CodeBehind.razor.cs");

        private ControlCodeBehindClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new ControlCodeBehindClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol,
                new TaskManagerService(),
                new WebFormMetricContext());
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = (await _converter.MigrateClassAsync()).Single();

            Assert.AreEqual(ExpectedOutputPath, fileInfo.RelativePath);
        }

        [Test]
        public async Task MigrateClassAsync_Properly_Sets_Base_Class()
        {
            var fileInfo = (await _converter.MigrateClassAsync()).Single();
            var text = Encoding.UTF8.GetString(fileInfo.FileBytes);

            var baseClass = SyntaxFactory.ParseSyntaxTree(text).GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single().BaseList.Types.Single().ToString();

            Assert.AreEqual(ExpectedBaseClass, baseClass);
        }
    }
}
