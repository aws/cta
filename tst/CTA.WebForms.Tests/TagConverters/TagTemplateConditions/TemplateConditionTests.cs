using CTA.WebForms.TagConverters.TagTemplateConditions;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateConditions
{
    public class TemplateConditionTests
    {
        [Test]
        public void ShouldCheckCondition_Returns_True_When_For_Templates_Contains_Template_Name()
        {
            var templateName = "TestTemplate1";

            var condition = new HasAttributeTemplateCondition()
            {
                ForTemplates = new[] { templateName }
            };

            Assert.True(condition.ShouldCheckCondition(templateName));
        }

        [Test]
        public void ShouldCheckCondition_Returns_True_When_For_Templates_Is_Empty()
        {
            var templateName = "TestTemplate1";

            var condition = new HasAttributeTemplateCondition()
            {
                ForTemplates = new string[] { }
            };

            Assert.True(condition.ShouldCheckCondition(templateName));
        }

        [Test]
        public void ShouldCheckCondition_Returns_True_When_For_Templates_Is_Not_Set()
        {
            var templateName = "TestTemplate1";

            var condition = new HasAttributeTemplateCondition();

            Assert.True(condition.ShouldCheckCondition(templateName));
        }

        [Test]
        public void ShouldCheckCondition_Returns_False_When_For_Templates_Does_Not_Contain_Template_Name()
        {
            var templateName = "TestTemplate1";
            var incorrectTemplateName = "TestTemplate2";

            var condition = new HasAttributeTemplateCondition()
            {
                ForTemplates = new[] { templateName }
            };

            Assert.False(condition.ShouldCheckCondition(incorrectTemplateName));
        }
    }
}
