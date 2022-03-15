using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.TagConverters.TagTemplateConditions;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateConditions
{
    [TestFixture]
    public class HasAttributeWithValueTemplateConditionTests
    {
        private HasAttributeWithValueTemplateCondition _basicCondition;
        private HasAttributeWithValueTemplateCondition _innerHtmlCondition;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _basicCondition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeName = "class",
                AttributeValue = "customclass"
            };

            _innerHtmlCondition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeName = "InnerHtml",
                AttributeValue = "Content"
            };
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_Attribute_Has_Correct_Value_In_Double_Quotes()
        {
            var node = HtmlNode.CreateNode("<p class=\"customclass\">Content</p>");

            Assert.True(_basicCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_Attribute_Has_Correct_Value_In_Single_Quotes()
        {
            var node = HtmlNode.CreateNode("<p class=\'customclass\'>Content</p>");

            Assert.True(_basicCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_Attribute_Has_Correct_Value_With_Surrounding_Spaces()
        {
            var node = HtmlNode.CreateNode("<p class=  \'customclass\'  >Content</p>");

            Assert.True(_basicCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Attribute_Has_Incorrect_Value()
        {
            var node = HtmlNode.CreateNode("<p class=\"otherclass\">Content</p>");

            Assert.False(_basicCondition.ConditionIsMet(node));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Node_Missing_Attribute()
        {
            var node = HtmlNode.CreateNode("<p>Content</p>");

            Assert.False(_basicCondition.ConditionIsMet(node));
        }

        // Note: InnerHtml is a special case, since it is technically not an attribute

        [Test]
        public void ConditionIsMet_Returns_True_When_Attribute_Is_Inner_Html_And_Inner_Html_Has_Correct_Value()
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
        public void Validate_Does_Not_Throw_Exception_For_Valid_Configuration_As_Base_Condition()
        {
            var condition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeName = "Attr0",
                AttributeValue = "Value0",
                ForTemplates = new[] { "Default" }
            };

            Assert.DoesNotThrow(() => condition.Validate(true));
        }

        [Test]
        public void Validate_Does_Not_Throw_Exception_For_Valid_Configuration_As_Non_Base_Condition()
        {
            var condition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeName = "Attr0",
                AttributeValue = "Value0"
            };

            Assert.DoesNotThrow(() => condition.Validate(false));
        }

        [Test]
        public void Validate_Throws_Exception_When_AttributeName_Missing()
        {
            var condition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeValue = "Value0"
            };

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(false));
        }

        [Test]
        public void Validate_Throws_Exception_When_AttributeValue_Missing()
        {
            var condition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeName = "Attr0"
            };


            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(true));
        }

        [Test]
        public void Validate_Throws_Exception_When_Not_Base_Condition_And_For_Templates_Has_Value()
        {
            var condition = new HasAttributeWithValueTemplateCondition()
            {
                AttributeName = "Attr0",
                AttributeValue = "Value0",
                ForTemplates = new[] { "Default" }
            };

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(false));
        }
    }
}
