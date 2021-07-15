using CTA.WebForms2Blazor.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ControlConverters
{
    public class UnknownControlConverterTests
    {
        private const string TestInputNodeStartTag = "<Test:Node Attr1=\"value1\" Attr2=\"value2\">";
        private const string TestInputNodeInnerHtml = "<p>Content</p>";
        private const string TestInputNodeEndTag = "</Test:Node>";

        private static string ExpectedNodeStartTag => $"<!-- Migration of this control not supported {TestInputNodeStartTag} -->";
        private static string ExpectedNodeEndTag => $"<!-- {TestInputNodeEndTag} -->";

        private static string ExpectedOutputText => $"<div>{ExpectedNodeStartTag}{ExpectedNodeEndTag}</div>";
        private static string ExpectedOutputTextWithContent => $"<div>{ExpectedNodeStartTag}{TestInputNodeInnerHtml}{ExpectedNodeEndTag}</div>";

        private static ControlConverter ControlConverter => new UnknownControlConverter();

        [Test]
        public void Convert2Blazor_Comments_Out_Control_Tags()
        {
            var testInputNode = HtmlNode.CreateNode(TestInputNodeStartTag + TestInputNodeEndTag);
            testInputNode.OwnerDocument.OptionOutputOriginalCase = true;
            var actualOutputNode = ControlConverter.Convert2Blazor(testInputNode);
            actualOutputNode.OwnerDocument.OptionOutputOriginalCase = true;

            Assert.AreEqual(ExpectedOutputText, actualOutputNode.WriteTo());
        }

        [Test]
        public void Convert2Blazor_Preserves_Content()
        {
            var testInputNodeWithContent = HtmlNode.CreateNode(TestInputNodeStartTag + TestInputNodeInnerHtml + TestInputNodeEndTag);
            testInputNodeWithContent.OwnerDocument.OptionOutputOriginalCase = true;
            var actualOutputNode = ControlConverter.Convert2Blazor(testInputNodeWithContent);
            actualOutputNode.OwnerDocument.OptionOutputOriginalCase = true;

            Assert.AreEqual(ExpectedOutputTextWithContent, actualOutputNode.WriteTo());
        }
    }
}
