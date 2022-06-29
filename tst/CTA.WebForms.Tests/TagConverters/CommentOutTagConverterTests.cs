using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.TagConverters;
using HtmlAgilityPack;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.TagConverters
{
    public class CommentOutTagConverterTests
    {
        [Test]
        public void Validate_Does_Not_Throw_Exception_When_Configuration_Is_Valid()
        {
            var converter = new CommentOutTagConverter()
            {
                TagName = "SomeTag"
            };

            Assert.DoesNotThrow(() => converter.Validate());
        }

        [Test]
        public void Validate_Throws_Exception_When_TagName_Not_Set()
        {
            var converter = new CommentOutTagConverter();

            Assert.Throws(typeof(ConfigValidationException), () => converter.Validate());
        }

        [Test]
        public async Task MigrateTagAsync_Works_On_Standard_Node()
        {
            var parent = HtmlNode.CreateNode(@"<div></div>");
            var child = HtmlNode.CreateNode(@"<p>Content...</p>");
            parent.AppendChild(child);

            var converter = new CommentOutTagConverter()
            {
                TagName = "p"
            };

            await converter.MigrateTagAsync(child, "testPath", null, 0);

            var expectedResult = "@*<p>Content...</p>*@";

            Assert.AreEqual(expectedResult, parent.InnerHtml);
        }

        [Test]
        public async Task MigrateTagAsync_Works_On_Node_With_Inner_Comments()
        {
            var parent = HtmlNode.CreateNode(@"<div></div>");
            var child = HtmlNode.CreateNode(@"<p>@*Content...*@</p>");
            parent.AppendChild(child);

            var converter = new CommentOutTagConverter()
            {
                TagName = "p"
            };

            await converter.MigrateTagAsync(child, "testPath", null, 0);

            var expectedResult = "@*<p>Content...</p>*@";

            Assert.AreEqual(expectedResult, parent.InnerHtml);
        }
    }
}
