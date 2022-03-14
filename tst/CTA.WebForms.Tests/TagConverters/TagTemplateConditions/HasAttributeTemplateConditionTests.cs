using CTA.WebForms.TagConverters.TagTemplateConditions;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateConditions
{
    [TestFixture]
    public class HasAttributeTemplateConditionTests
    {
        private HasAttributeTemplateCondition _basicCondition;
        private HasAttributeTemplateCondition _innerHtmlCondition;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _basicCondition = new HasAttributeTemplateCondition()
            {
                AttributeName = "class",
            };

            _innerHtmlCondition = new HasAttributeTemplateCondition()
            {
                AttributeName = "InnerHtml"
            };
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_Node_Has_Attribute()
        {
            var node = HtmlNode.CreateNode("<p class=\"customclass\">Content</p>");

            Assert.True(_basicCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Node_Missing_Attribute()
        {
            var node = HtmlNode.CreateNode("<p>Content</p>");

            Assert.False(_basicCondition.ConditionIsMet(node));
        }

        // Note: InnerHtml is a special case, since it is technically not an attribute

        [Test]
        public void ConditionIsMet_Returns_True_When_Attribute_Is_Inner_Html_And_Inner_Html_Has_Valid_Value()
        {
            var node = HtmlNode.CreateNode("<p>Content</p>");

            Assert.True(_innerHtmlCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Attribute_Is_Inner_Html_And_Missing_Inner_Html()
        {
            var node = HtmlNode.CreateNode("<input type=\"text\"/>");

            Assert.False(_innerHtmlCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Attribute_Is_Inner_Html_And_Inner_Html_Is_Empty_String()
        {
            var node = HtmlNode.CreateNode("<p></p>");

            Assert.False(_innerHtmlCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Attribute_Is_Inner_Html_And_Inner_Html_Is_Empty_Space()
        {
            var node = HtmlNode.CreateNode("<p>\n    \n\t</p>");

            Assert.False(_innerHtmlCondition.ConditionIsMet(node));
        }

        [Test]
        public void Validate_Returns_True_For_Valid_Configuration_As_Base_Condition()
        {
            var condition = new HasAttributeTemplateCondition()
            {
                AttributeName = "Attr0",
                ForTemplates = new[] { "Default" }
            };

            Assert.True(condition.Validate(true));
        }

        [Test]
        public void Validate_Returns_True_For_Valid_Configuration_As_Non_Base_Condition()
        {
            var condition = new HasAttributeTemplateCondition()
            {
                AttributeName = "Attr0"
            };

            Assert.True(condition.Validate(false));
        }

        [Test]
        public void Validate_Returns_False_When_AttributeName_Missing()
        {
            var condition = new HasAttributeTemplateCondition();

            Assert.False(condition.Validate(false));
        }

        [Test]
        public void Validate_Returns_False_When_Not_Base_Condition_And_For_Templates_Has_Value()
        {
            var condition = new HasAttributeTemplateCondition()
            {
                AttributeName = "Attr0",
                ForTemplates = new[] { "Default" }
            };

            Assert.False(condition.Validate(false));
        }
    }
}
