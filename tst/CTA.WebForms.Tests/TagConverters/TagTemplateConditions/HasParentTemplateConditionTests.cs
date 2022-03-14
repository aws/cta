using CTA.WebForms.TagConverters.TagTemplateConditions;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateConditions
{
    [TestFixture]
    public class HasParentTemplateConditionTests
    {
        private HasParentTemplateCondition _condition;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _condition = new HasParentTemplateCondition()
            {
                ParentTagName = "div"
            };
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_Node_Has_Correct_Parent()
        {
            var parent = HtmlNode.CreateNode("<div></div>");
            var child = HtmlNode.CreateNode("<p>Content</p>");

            parent.AppendChild(child);

            Assert.True(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Node_Has_Incorrect_Parent()
        {
            var parent = HtmlNode.CreateNode("<span></span>");
            var child = HtmlNode.CreateNode("<p>Content</p>");

            parent.AppendChild(child);

            Assert.False(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Parent_Node_Doesnt_Exist()
        {
            var child = HtmlNode.CreateNode("<p>Content</p>");

            Assert.False(_condition.ConditionIsMet(child));
        }

        [Test]
        public void Validate_Returns_True_For_Valid_Configuration_As_Base_Condition()
        {
            var condition = new HasParentTemplateCondition()
            {
                ParentTagName = "div",
                ForTemplates = new[] { "Default" }
            };

            Assert.True(condition.Validate(true));
        }

        [Test]
        public void Validate_Returns_True_For_Valid_Configuration_As_Non_Base_Condition()
        {
            var condition = new HasParentTemplateCondition()
            {
                ParentTagName = "div"
            };

            Assert.True(condition.Validate(false));
        }

        [Test]
        public void Validate_Returns_False_When_ParentTagName_Missing()
        {
            var condition = new HasParentTemplateCondition();

            Assert.False(condition.Validate(false));
        }

        [Test]
        public void Validate_Returns_False_When_Not_Base_Condition_And_For_Templates_Has_Value()
        {
            var condition = new HasParentTemplateCondition()
            {
                ParentTagName = "div",
                ForTemplates = new[] { "Default" }
            };

            Assert.False(condition.Validate(false));
        }
    }
}
