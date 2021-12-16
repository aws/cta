using CTA.WebForms.Helpers.ControlHelpers;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.Helpers.ControlHelpers
{
    public class UnknownControlRemoverTests
    {
        private const string TestInputNodeStartTag = "<Test:Node Attr1=\"value1\" Attr2=\"value2\">";
        private const string TestInputNodeInnerHtml = "<p>Content</p>";
        private const string TestInputNodeEndTag = "</Test:Node>";

        private static string ExpectedNodeStartTag => $"@* The following tag is not supported: {TestInputNodeStartTag} *@";
        private static string ExpectedNodeEndTag => $"@* {TestInputNodeEndTag} *@";

        [Test]
        public void Convert2Blazor_Comments_Out_Control_Tags()
        {
            var testInput =
$@"{TestInputNodeStartTag}
{TestInputNodeEndTag}";
            var expectedOutput =
$@"{ExpectedNodeStartTag}
{ExpectedNodeEndTag}";
            var actualOutput = UnknownControlRemover.RemoveUnknownTags(testInput);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void Convert2Blazor_Preserves_Content()
        {
            var testInput =
$@"{TestInputNodeStartTag}
    {TestInputNodeInnerHtml}
{TestInputNodeEndTag}";
            var expectedOutput =
$@"{ExpectedNodeStartTag}
    {TestInputNodeInnerHtml}
{ExpectedNodeEndTag}";
            var actualOutput = UnknownControlRemover.RemoveUnknownTags(testInput);

            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
