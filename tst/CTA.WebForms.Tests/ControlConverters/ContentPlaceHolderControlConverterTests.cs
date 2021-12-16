using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ControlConverters
{
    public class ContentPlaceHolderControlConverterTests
    { 
        [Test]
        public void ContentPlaceHolderControlConverter_Returns_Body_Directive()
        {
            var htmlString = @"<asp:ContentPlaceHolder ID=""MainContent"" runat=""server"">
    </asp:ContentPlaceHolder>";
            var htmlNode = HtmlNode.CreateNode(htmlString);
            htmlNode.OwnerDocument.OptionOutputOriginalCase = true;
            var testContentPlaceHolderControlConverter = new ContentPlaceHolderControlConverter();
            var convertedNode = testContentPlaceHolderControlConverter.Convert2Blazor(htmlNode);

            var actualString = convertedNode.WriteTo();
            var expectedString = @"@Body";
            
            Assert.AreEqual(expectedString, actualString);
        }
    }
}