using CTA.WebForms2Blazor.ClassConverters;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class HttpModuleClassConverterTests
    {
        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "HttpModule.cs");
        private static string ExpectedOutputPath => Path.Combine("Middleware", ClassConverterSetupFixture.TestProjectNestedDirectoryName, "HttpModule.cs");

        private HttpHandlerClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new HttpHandlerClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol);
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = await _converter.MigrateClassAsync();

            Assert.AreEqual(ExpectedOutputPath, fileInfo.RelativePath);
        }
    }
}
