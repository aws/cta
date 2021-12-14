using CTA.WebForms2Blazor.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ControlConverters
{
    public class HyperLinkControlConverterTests
    {
        private HyperLinkControlConverter _testConverter;

        [SetUp]
        public void SetUp()
        {
            _testConverter = new HyperLinkControlConverter();
        }

        [Test]
        public void Convert2Blazor_Properly_Sets_Inner_Html_When_Not_Originally_Present()
        {
            var htmlString = @"<asp:HyperLink ID=""TestId"" CssClass=""TestClass"" NavigateUrl=""OtherPage"" Text=""Test Link Text""/>";
            var expectedString = @"<a id=""TestId"" class=""TestClass"" href=""OtherPage"">Test Link Text</a>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Properly_Sets_Inner_Html_When_Already_Present()
        {
            var htmlString = @"<asp:HyperLink ID=""TestId"" CssClass=""TestClass"" NavigateUrl=""OtherPage"">Test Link Text</asp:HyperLink>";
            var expectedString = @"<a id=""TestId"" class=""TestClass"" href=""OtherPage"">Test Link Text</a>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Prioritizes_Inner_Html_Over_Text_Attribute()
        {
            var htmlString = @"<asp:HyperLink ID=""TestId"" CssClass=""TestClass"" NavigateUrl=""OtherPage"" Text=""Test Link Text 1"">Test Link Text 2</asp:HyperLink>";
            var expectedString = @"<a id=""TestId"" class=""TestClass"" href=""OtherPage"">Test Link Text 2</a>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }
    }
}
