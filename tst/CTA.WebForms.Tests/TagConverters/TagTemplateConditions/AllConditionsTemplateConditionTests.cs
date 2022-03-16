using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.TagConverters.TagTemplateConditions;
using HtmlAgilityPack;
using NUnit.Framework;
using System;

namespace CTA.WebForms.Tests.TagConverters.TagTemplateConditions
{
    [TestFixture]
    public class AllConditionsTemplateConditionTests
    {
        private AllConditionsTemplateCondition _condition;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var hasAttributeCondition = new HasAttributeTemplateCondition()
            {
                AttributeName = "class"
            };
            var hasParentCondition = new HasParentTemplateCondition()
            {
                ParentTagName = "span"
            };
            var hasGrandparentCondition = new HasGrandparentTemplateCondition()
            {
                GrandparentTagName = "div"
            };

            _condition = new AllConditionsTemplateCondition()
            {
                Conditions = new TemplateCondition[]
                {
                    hasAttributeCondition,
                    hasParentCondition,
                    hasGrandparentCondition
                }
            };
        }

        [Test]
        public void ConditionIsMet_Returns_True_When_All_Sub_Conditions_Are_Met()
        {
            var grandparent = HtmlNode.CreateNode("<div></div>");
            var parent = HtmlNode.CreateNode("<span></span>");
            var child = HtmlNode.CreateNode("<p class=\"customclass\">Content</p>");

            grandparent.AppendChild(parent);
            parent.AppendChild(child);

            Assert.True(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_One_Sub_Condition_Is_Not_Met()
        {
            var grandparent = HtmlNode.CreateNode("<div></div>");
            var parent = HtmlNode.CreateNode("<div></div>");
            var child = HtmlNode.CreateNode("<p class=\"customclass\">Content</p>");

            grandparent.AppendChild(parent);
            parent.AppendChild(child);

            Assert.False(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_Some_Sub_Conditions_Are_Not_Met()
        {
            var grandparent = HtmlNode.CreateNode("<span></span>");
            var parent = HtmlNode.CreateNode("<div></div>");
            var child = HtmlNode.CreateNode("<p class=\"customclass\">Content</p>");

            grandparent.AppendChild(parent);
            parent.AppendChild(child);

            Assert.False(_condition.ConditionIsMet(child));
        }

        [Test]
        public void ConditionIsMet_Returns_False_When_All_Sub_Conditions_Are_Not_Met()
        {
            var grandparent = HtmlNode.CreateNode("<span></span>");
            var parent = HtmlNode.CreateNode("<div></div>");
            var child = HtmlNode.CreateNode("<p>Content</p>");

            grandparent.AppendChild(parent);
            parent.AppendChild(child);

            Assert.False(_condition.ConditionIsMet(child));
        }


        [Test]
        public void Validate_Does_Not_Throw_Exception_For_Valid_Configuration_As_Base_Condition()
        {
            var condition = new AllConditionsTemplateCondition()
            {
                Conditions = new[] { new HasAttributeTemplateCondition() { AttributeName = "Attr0" } },
                ForTemplates = new[] { "Default" }
            };

            Assert.DoesNotThrow(() => condition.Validate(true));
        }

        [Test]
        public void Validate_Does_Not_Throw_Exception_For_Valid_Configuration_As_Non_Base_Condition()
        {
            var condition = new AllConditionsTemplateCondition()
            {
                Conditions = new[] { new HasAttributeTemplateCondition() { AttributeName = "Attr0" } }
            };

            Assert.DoesNotThrow(() => condition.Validate(false));
        }

        [Test]
        public void Validate_Throws_Exception_When_Conditions_Missing()
        {
            var condition = new AllConditionsTemplateCondition();

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(false));
        }

        [Test]
        public void Validate_Throws_Exception_When_Conditions_Empty()
        {
            var condition = new AllConditionsTemplateCondition()
            {
                Conditions = new TemplateCondition[] { }
            };

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(false));
        }

        [Test]
        public void Validate_Throws_Exception_When_Sub_Condition_Validate_Fails()
        {
            var condition = new AllConditionsTemplateCondition()
            {
                Conditions = new[] {
                    new HasGrandparentTemplateCondition()
                },
            };

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(true));
        }

        [Test]
        public void Validate_Throws_Exception_When_Sub_Condition_Validate_Fails_And_Others_Succeed()
        {
            var condition = new AllConditionsTemplateCondition()
            {
                Conditions = new TemplateCondition[] {
                    new HasGrandparentTemplateCondition() { GrandparentTagName = "div" },
                    new HasParentTemplateCondition(),
                    new HasAttributeTemplateCondition() { AttributeName = "Attr0" }
                },
            };

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(true));
        }

        [Test]
        public void Validate_Throws_Exception_When_Not_Base_Condition_And_For_Templates_Has_Value()
        {
            var condition = new AllConditionsTemplateCondition()
            {
                Conditions = new[] { new HasAttributeTemplateCondition() { AttributeName = "Attr0" } },
                ForTemplates = new[] { "Default" }
            };

            Assert.Throws(typeof(ConfigValidationException), () => condition.Validate(false));
        }
    }
}
