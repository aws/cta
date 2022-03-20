using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using CTA.WebForms.TagConverters.TagTemplateInvokables;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateInvokables
{
    [TestFixture]
    public class AddUsingDirectiveTemplateInvokableTests
    {
        private ViewImportService _viewImportService;

        [SetUp]
        public void SetUp()
        {
            _viewImportService = new ViewImportService();
        }

        [Test]
        public void Invoke_Adds_Using_Directive_To_Service()
        {
            var namespaceName = "Name.Space.Name";
            var invokable = new AddUsingDirectiveTemplateInvokable()
            {
                NamespaceName = namespaceName
            };

            invokable.Initialize(_viewImportService);

            invokable.Invoke();

            Assert.True(_viewImportService.ViewUsingDirectives.Contains($"@using {namespaceName}"));
        }

        [Test]
        public void Validate_Does_Not_Throw_Exception_For_Valid_Configuration()
        {
            var invokable = new AddUsingDirectiveTemplateInvokable()
            {
                NamespaceName = "Name.Space.Name"
            };

            Assert.DoesNotThrow(() => invokable.Validate());
        }

        [Test]
        public void Validate_Throws_Exception_When_NamespaceName_Missing()
        {
            var invokable = new AddUsingDirectiveTemplateInvokable();

            Assert.Throws(typeof(ConfigValidationException), () => invokable.Validate());
        }

        [Test]
        public void Validate_Throws_Exception_When_NamespaceName_Empty()
        {
            var invokable = new AddUsingDirectiveTemplateInvokable()
            {
                NamespaceName = string.Empty
            };

            Assert.Throws(typeof(ConfigValidationException), () => invokable.Validate());
        }
    }
}
