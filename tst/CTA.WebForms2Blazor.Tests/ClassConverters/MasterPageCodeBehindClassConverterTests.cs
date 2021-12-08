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
    public class MasterPageCodeBehindClassConverterTests
    {
        private const string ExpectedBaseClass = "LayoutComponentBase";
        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "CodeBehind.Master.cs");
        private static string ExpectedOutputPath => Path.Combine("Layouts", ClassConverterSetupFixture.TestProjectNestedDirectoryName, "CodeBehind.razor.cs");

        private MasterPageCodeBehindClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            MetricsContext context = new MetricsContext(ClassConverterSetupFixture.TestProjectDirectoryPath);
            _converter = new MasterPageCodeBehindClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol,
                new TaskManagerService(),
                new WebFormMetricContext(context, ClassConverterSetupFixture.TestProjectDirectoryPath));
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
