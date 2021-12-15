using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ControlConverters
{
    public class ListViewControlConverterTests
    {
        [Test]
        public void ListViewControlConverter_Returns_ListView_Node()
        {
            var htmlString = @"<asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
    <LayoutTemplate>
        <table class=""table"">
            <tbody>
                <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder""></asp:PlaceHolder>
            </tbody>
        </table>
    </LayoutTemplate>
</asp:ListView>";
            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var testListViewControlConverter = new ListViewControlConverter();
            var convertedNode = testListViewControlConverter.Convert2Blazor(htmlNode);

            var actualString = convertedNode.WriteTo();
            var expectedString = @"<ListView @ref=""productList"" ItemPlaceholderID=""itemPlaceHolder"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"" Context=""Item"">
    <LayoutTemplate Context=""itemPlaceHolder"">
        <table class=""table"">
            <tbody>
                @itemPlaceHolder
            </tbody>
        </table>
    </LayoutTemplate>
</ListView>";
            
            Assert.AreEqual(expectedString, actualString);
        }
    }
}