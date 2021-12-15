using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ControlConverters
{
    public class LabelControlConverterTests
    {
        private LabelControlConverter _testConverter;

        [SetUp]
        public void SetUp()
        {
            _testConverter = new LabelControlConverter();
        }

        [Test]
        public void Convert2Blazor_Properly_Sets_Inner_Html_When_Not_Originally_Present()
        {
            var htmlString = @"<asp:Label ID=""TestId"" CssClass=""TestClass"" Text=""Test Label Text""/>";
            var expectedString = @"<label id=""TestId"" class=""TestClass"">Test Label Text</label>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Properly_Sets_Inner_Html_When_Already_Present()
        {
            var htmlString = @"<asp:Label ID=""TestId"" CssClass=""TestClass"">Test Label Text</asp:Label>";
            var expectedString = @"<label id=""TestId"" class=""TestClass"">Test Label Text</label>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Prioritizes_Inner_Html_Over_Text_Attribute()
        {
            var htmlString = @"<asp:Label ID=""TestId"" CssClass=""TestClass"" Text=""Test Label Text 1"">Test Label Text 2</asp:Label>";
            var expectedString = @"<label id=""TestId"" class=""TestClass"">Test Label Text 2</label>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }
    }
}
