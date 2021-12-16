using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ControlConverters
{
    public class ButtonControlConverterTests
    {
        private ButtonControlConverter _testConverter;

        [SetUp]
        public void SetUp()
        {
            _testConverter = new ButtonControlConverter();
        }

        [Test]
        public void Convert2Blazor_Properly_Converts_Present_Attributes()
        {
            var htmlString = @"<asp:Button ID=""TestId"" CssClass=""TestClass"" Text=""Test Button Text""/>";
            var expectedString = @"<button id=""TestId"" class=""TestClass"">Test Button Text</button>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Sets_Disabled_Attribute_Properly()
        {
            var htmlString = @"<asp:Button ID=""TestId"" CssClass=""TestClass"" Text=""Test Button Text"" Enabled=""False""/>";
            var expectedString = @"<button id=""TestId"" class=""TestClass"" disabled="""">Test Button Text</button>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }
    }
}
