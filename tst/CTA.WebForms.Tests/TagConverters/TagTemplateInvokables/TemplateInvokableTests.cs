using CTA.WebForms.TagConverters.TagTemplateInvokables;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateInvokables
{
    public class TemplateInvokableTests
    {
        [Test]
        public void ShouldCheckCondition_Returns_True_When_For_Templates_Contains_Template_Name()
        {
            var templateName = "TestTemplate1";

            var condition = new AddUsingDirectiveTemplateInvokable()
            {
                ForTemplates = new[] { templateName }
            };

            Assert.True(condition.ShouldInvoke(templateName));
        }

        [Test]
        public void ShouldCheckCondition_Returns_True_When_For_Templates_Is_Empty()
        {
            var templateName = "TestTemplate1";

            var condition = new AddUsingDirectiveTemplateInvokable()
            {
                ForTemplates = new string[] { }
            };

            Assert.True(condition.ShouldInvoke(templateName));
        }

        [Test]
        public void ShouldCheckCondition_Returns_True_When_For_Templates_Is_Not_Set()
        {
            var templateName = "TestTemplate1";

            var condition = new AddUsingDirectiveTemplateInvokable();

            Assert.True(condition.ShouldInvoke(templateName));
        }

        [Test]
        public void ShouldCheckCondition_Returns_False_When_For_Templates_Does_Not_Contain_Template_Name()
        {
            var templateName = "TestTemplate1";
            var incorrectTemplateName = "TestTemplate2";

            var condition = new AddUsingDirectiveTemplateInvokable()
            {
                ForTemplates = new[] { templateName }
            };

            Assert.False(condition.ShouldInvoke(incorrectTemplateName));
        }
    }
}
