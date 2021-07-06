using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class PageCodeBehindClassConverterTests
    {
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
                new TaskManagerService());
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = await _converter.MigrateClassAsync();

            Assert.AreEqual(ExpectedOutputPath, fileInfo.RelativePath);
        }
    }
}
