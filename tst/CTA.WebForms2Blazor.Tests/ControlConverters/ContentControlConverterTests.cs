using CTA.WebForms2Blazor.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ControlConverters
{
    public class ContentControlConverterTests
    {
        [Test]
        public void ContentControlConverter_Returns_Inner_Nodes()
        {
            var htmlString = @"<asp:Content ID=""CatalogList"" ContentPlaceHolderID=""MainContent"" runat=""server"">
    <div class=""esh-pager"">
        <div class=""container"">
            <article class=""esh-pager-wrapper row"">
                <nav>
                    <asp:HyperLink ID=""PaginationPrevious"" runat=""server"" CssClass=""esh-pager-item esh-pager-item--navigable"">
                        Previous
                    </asp:HyperLink>
                </nav>
            </article>
        </div>
    </div>
    <div>
        <p> Some random stuff </p>
    </div>
</asp:Content>";
            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var testContentControlConverter = new ContentControlConverter();
            var convertedNode = testContentControlConverter.Convert2Blazor(htmlNode);

            var actualString = convertedNode.WriteTo();
            var expectedString = @"<div>
    <div class=""esh-pager"">
        <div class=""container"">
            <article class=""esh-pager-wrapper row"">
                <nav>
                    <asp:HyperLink ID=""PaginationPrevious"" runat=""server"" CssClass=""esh-pager-item esh-pager-item--navigable"">
                        Previous
                    </asp:HyperLink>
                </nav>
            </article>
        </div>
    </div>
    <div>
        <p> Some random stuff </p>
    </div>
</div>";
            
            Assert.AreEqual(expectedString, actualString);
        }
    }
}