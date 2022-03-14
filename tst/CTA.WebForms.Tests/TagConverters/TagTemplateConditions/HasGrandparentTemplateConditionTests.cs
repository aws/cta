using CTA.WebForms.TagConverters.TagTemplateConditions;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateConditions
{
    [TestFixture]
    public class HasGrandparentTemplateConditionTests
    {
        private HasGrandparentTemplateCondition _condition;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _condition = new HasGrandparentTemplateCondition()
            {
                GrandparentTagName = "div"
            };
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_Node_Has_Correct_Grandparent()
        {
            var grandparent = HtmlNode.CreateNode("<div></div>");
            var parent = HtmlNode.CreateNode("<span></span>");
            var child = HtmlNode.CreateNode("<p>Content</p>");

            grandparent.AppendChild(parent);
            parent.AppendChild(child);

            Assert.True(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Node_Has_Incorrect_Grandparent()
        {
            var grandparent = HtmlNode.CreateNode("<span></span>");
            var parent = HtmlNode.CreateNode("<div></div>");
            var child = HtmlNode.CreateNode("<p>Content</p>");

            grandparent.AppendChild(parent);
            parent.AppendChild(child);

            Assert.False(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Grandparent_Node_Doesnt_Exist()
        {
            var parent = HtmlNode.CreateNode("<div></div>");
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
            var condition = new HasGrandparentTemplateCondition()
            {
                GrandparentTagName = "div",
                ForTemplates = new[] { "Default" }
            };

            Assert.True(condition.Validate(true));
        }

        [Test]
        public void Validate_Returns_True_For_Valid_Configuration_As_Non_Base_Condition()
        {
            var condition = new HasGrandparentTemplateCondition()
            {
                GrandparentTagName = "div"
            };

            Assert.True(condition.Validate(false));
        }

        [Test]
        public void Validate_Returns_False_When_GrandparentTagName_Missing()
        {
            var condition = new HasGrandparentTemplateCondition();

            Assert.False(condition.Validate(false));
        }

        [Test]
        public void Validate_Returns_False_When_Not_Base_Condition_And_For_Templates_Has_Value()
        {
            var condition = new HasGrandparentTemplateCondition()
            {
                GrandparentTagName = "div",
                ForTemplates = new[] { "Default" }
            };

            Assert.False(condition.Validate(false));
        }
    }
}
