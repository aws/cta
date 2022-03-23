using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using CTA.WebForms.TagConverters.TagTemplateInvokables;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateInvokables
{
    [TestFixture]
    public class AddNugetPackageTemplateInvokableTests
    {
        private ViewImportService _viewImportService;

        [SetUp]
        public void SetUp()
        {
            _viewImportService = new ViewImportService();
        }

        [Test]
        public void Invoke_Adds_NuGet_Package_To_Service()
        {
            var namespaceName = "NuGet.Package.Name";
            var invokable = new AddNugetPackageTemplateInvokable()
            {
                PackageName = namespaceName
            };

            invokable.Initialize(_viewImportService);

            invokable.Invoke();

            Assert.True(_viewImportService.NewNuGetPackages.Contains(namespaceName));
        }

        [Test]
        public void Validate_Does_Not_Throw_Exception_For_Valid_Configuration()
        {
            var invokable = new AddNugetPackageTemplateInvokable()
            {
                PackageName = "NuGet.Package.Name"
            };

            Assert.DoesNotThrow(() => invokable.Validate());
        }

        [Test]
        public void Validate_Throws_Exception_When_PackageName_Missing()
        {
            var invokable = new AddNugetPackageTemplateInvokable();

            Assert.Throws(typeof(ConfigValidationException), () => invokable.Validate());
        }

        [Test]
        public void Validate_Throws_Exception_When_PackageName_Empty()
        {
            var invokable = new AddNugetPackageTemplateInvokable()
            {
                PackageName = string.Empty
            };

            Assert.Throws(typeof(ConfigValidationException), () => invokable.Validate());
        }
    }
}
