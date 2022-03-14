using CTA.WebForms.Services;
using CTA.WebForms.TagConverters;
using CTA.WebForms.TagConverters.TagTemplateConditions;
using CTA.WebForms.TagConverters.TagTemplateInvokables;
using HtmlAgilityPack;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.WebForms.Tests.TagConverters
{
    public class TemplateTagConverterTests
    {
        [Test]
        public void Validate_Returns_True_When_Configuration_Is_Valid()
        {
            var templateConverter = new TemplateTagConverter()
            {
                CodeBehindHandler = "Default",
                Conditions = new[]
                {
                    new HasAttributeTemplateCondition() { AttributeName = "Attr0" }
                },
                Invocations = new[]
                {
                    new AddUsingDirectiveTemplateInvokable() { NamespaceName = "Namespace0" }
                },
                Templates = new Dictionary<string, string>
                {
                    { "Default", "<p>#Attr0#</p>" }
                }
            };

            Assert.True(templateConverter.Validate());
        }

        [Test]
        public void Validate_Returns_True_When_Optional_Properties_Not_Set()
        {
            var templateConverter = new TemplateTagConverter()
            {
                Templates = new Dictionary<string, string>
                {
                    { "Default", "<p>#Attr0#</p>" }
                }
            };

            Assert.True(templateConverter.Validate());
        }

        [Test]
        public void Validate_Returns_False_When_CodeBehindHandler_Not_Valid()
        {
            var templateConverter = new TemplateTagConverter()
            {
                CodeBehindHandler = "NonExistentType",
            };

            Assert.False(templateConverter.Validate());
        }

        [Test]
        public void Validate_Returns_False_When_Some_Conditions_Not_Valid()
        {
            var templateConverter = new TemplateTagConverter()
            {
                CodeBehindHandler = "Default",
                Conditions = new[]
                {
                    new HasAttributeTemplateCondition()
                },
                Invocations = new[]
                {
                    new AddUsingDirectiveTemplateInvokable() { NamespaceName = "Namespace0" }
                },
                Templates = new Dictionary<string, string>
                {
                    { "Default", "<p>#Attr0#</p>" }
                }
            };

            Assert.False(templateConverter.Validate());
        }

        [Test]
        public void Validate_Returns_False_When_Some_Invocations_Not_Valid()
        {
            var templateConverter = new TemplateTagConverter()
            {
                CodeBehindHandler = "Default",
                Conditions = new[]
                {
                    new HasAttributeTemplateCondition() { AttributeName = "Attr0" }
                },
                Invocations = new[]
                {
                    new AddUsingDirectiveTemplateInvokable()
                },
                Templates = new Dictionary<string, string>
                {
                    { "Default", "<p>#Attr0#</p>" }
                }
            };

            Assert.False(templateConverter.Validate());
        }

        [Test]
        public void MigrateTag_Replaces_Node_Using_Correct_Template()
        {
            var expectedInnerHtml = "<div><p>https://aws.amazon.com</p></div>";

            var parent = HtmlNode.CreateNode("<div></div>");
            var node = HtmlNode.CreateNode("<a href=\"https://aws.amazon.com\"");

            parent.AppendChild(node);

            var templateConverter = new TemplateTagConverter()
            {
                Conditions = new[]
                {
                    new HasAttributeTemplateCondition() {
                        AttributeName = "Attr0",
                        ForTemplates = new[] { "Other0" }
                    },
                    new HasAttributeTemplateCondition()
                    {
                        AttributeName = "href",
                        ForTemplates = new[] { "Other1" }
                    }
                },
                Templates = new Dictionary<string, string>
                {
                    { "Other0", "<p>#Attr0#</p>" },
                    { "Other1", "<div><p>#href#</p></div>" },
                    { "Default", "<span>This is a placeholder for your link...</span>" }
                }
            };
            templateConverter.Initialize(new CodeBehindReferenceLinkerService());

            templateConverter.MigrateTag(node);

            Assert.AreEqual(expectedInnerHtml, parent.InnerHtml);
        }

        [Test]
        public void MigrateTag_Does_Nothing_When_No_Suitable_Template_Is_Found()
        {
            var expectedInnerHtml = "<span></span>";

            var parent = HtmlNode.CreateNode("<div></div>");
            var node = HtmlNode.CreateNode(expectedInnerHtml);

            parent.AppendChild(node);

            var templateConverter = new TemplateTagConverter()
            {
                Conditions = new[]
                {
                    new HasAttributeTemplateCondition() {
                        AttributeName = "Attr0",
                        ForTemplates = new[] { "Default" }
                    }
                },
                Templates = new Dictionary<string, string>
                {
                    { "Default", "<p>#Attr0#</p>" }
                }
            };

            templateConverter.MigrateTag(node);

            Assert.AreEqual(expectedInnerHtml, parent.InnerHtml);
        }
    }
}
