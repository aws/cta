using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms2Blazor.Metrics;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class UnknownClassConverterTests
    {
        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "UnknownClass.cs");

        private UnknownClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            MetricsContext context = new MetricsContext(ClassConverterSetupFixture.TestProjectDirectoryPath);
            _converter = new UnknownClassConverter(InputRelativePath,
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
            var expectedOutputPath = $"{Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, ClassConverterSetupFixture.TestClassName)}.cs";

            // Relative path should stay the same
            Assert.AreEqual(expectedOutputPath, fileInfo.RelativePath);
        }
    }
}
