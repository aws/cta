using CTA.WebForms2Blazor.ClassConverters;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class UnknownClassConverterTests
    {
        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "UnknownClass.cs");

        private UnknownClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new UnknownClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol);
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = await _converter.MigrateClassAsync();

            // Relative path should stay the same
            Assert.AreEqual(InputRelativePath, fileInfo.RelativePath);
        }
    }
}
