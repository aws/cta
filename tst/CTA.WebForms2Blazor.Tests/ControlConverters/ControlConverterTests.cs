using CTA.WebForms2Blazor.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ControlConverters
{
    public class ControlConverterTests
    {
        private const string TestUpdateInnerHtmlNodeHtmlString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <LayoutTemplate>
            <table class=""table"">
                <tbody>
                    <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder""></asp:PlaceHolder>
                </tbody>
            </table>
        </LayoutTemplate>
    </asp:ListView>
</div>";
        private const string TestMultipleUpdateInnerHtmlNodeHtmlString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <LayoutTemplate>
            <table class=""table"">
                <tbody>
                    <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder""></asp:PlaceHolder>
                    <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder1""></asp:PlaceHolder>
                    <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder2""></asp:PlaceHolder>
                </tbody>
            </table>
        </LayoutTemplate>
    </asp:ListView>
</div>";
        private ControlConverter _testControlConverter;

        [SetUp]
        public void Setup()
        {
            _testControlConverter = new ListViewControlConverter();
        }

        [Test]
        public void TestUpdateInnerHtmlNode_Succeeds_On_Name_And_Id_Match()
        {
            var htmlNode = HtmlNode.CreateNode(TestUpdateInnerHtmlNodeHtmlString);
            htmlNode.OwnerDocument.OptionOutputOriginalCase = true;
            var correctNameAndId = _testControlConverter.UpdateInnerHtmlNode(htmlNode, "asp:PlaceHolder", 
                "itemPlaceHolder", template: "@{2}", newBody: "itemPlaceHolder");

            var actualString = htmlNode.WriteTo();
            var expectedString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <LayoutTemplate>
            <table class=""table"">
                <tbody>
                    @itemPlaceHolder
                </tbody>
            </table>
        </LayoutTemplate>
    </asp:ListView>
</div>";
            
            Assert.True(correctNameAndId);
            Assert.AreEqual(expectedString, actualString);
        }
        
        [Test]
        public void TestUpdateInnerHtmlNode_Succeeds_On_Name_Match_Only()
        {
            var htmlNode = HtmlNode.CreateNode(TestUpdateInnerHtmlNodeHtmlString);
            htmlNode.OwnerDocument.OptionOutputOriginalCase = true;
            var correctNameAndNoId = _testControlConverter.UpdateInnerHtmlNode(htmlNode, "asp:PlaceHolder",
                "itemPlaceHolder", template: "@{2}", newBody: "itemPlaceHolder");
            
            var actualString = htmlNode.WriteTo();
            var expectedString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <LayoutTemplate>
            <table class=""table"">
                <tbody>
                    @itemPlaceHolder
                </tbody>
            </table>
        </LayoutTemplate>
    </asp:ListView>
</div>";
            
            Assert.True(correctNameAndNoId);
            Assert.AreEqual(expectedString, actualString);
        }
        
        [Test]
        public void TestUpdateInnerHtmlNode_Fails_On_Correct_Name_No_Id_Match()
        {
            var htmlNode = HtmlNode.CreateNode(TestUpdateInnerHtmlNodeHtmlString);
            htmlNode.OwnerDocument.OptionOutputOriginalCase = true;
            var correctNameAndIncorrectId = _testControlConverter.UpdateInnerHtmlNode(htmlNode, "asp:PlaceHolder", "inccorectid");
            var resultString = htmlNode.WriteTo();
            
            Assert.False(correctNameAndIncorrectId);
            Assert.AreEqual(TestUpdateInnerHtmlNodeHtmlString, resultString);
        }

        [Test]
        public void TestUpdateInnerHtmlNode_Multiple_Matches()
        {
            var htmlNode = HtmlNode.CreateNode(TestMultipleUpdateInnerHtmlNodeHtmlString);
            htmlNode.OwnerDocument.OptionOutputOriginalCase = true;
            var multipleNameMatches = _testControlConverter.UpdateInnerHtmlNode(htmlNode, "asp:PlaceHolder", 
                template: "@{2}", newBody: "itemPlaceHolder");

            var actualString = htmlNode.WriteTo();
            var expectedString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <LayoutTemplate>
            <table class=""table"">
                <tbody>
                    @itemPlaceHolder
                    @itemPlaceHolder
                    @itemPlaceHolder
                </tbody>
            </table>
        </LayoutTemplate>
    </asp:ListView>
</div>";
            
            Assert.True(multipleNameMatches);
            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void TestUpdateInnerHtmlNode_Multiple_Name_Match_Single_Id_Match()
        {
            var htmlNode = HtmlNode.CreateNode(TestMultipleUpdateInnerHtmlNodeHtmlString);
            htmlNode.OwnerDocument.OptionOutputOriginalCase = true;
            var multipleNameMatchSingleIdMatch = _testControlConverter.UpdateInnerHtmlNode(htmlNode, "asp:PlaceHolder", 
                id: "itemPlaceHolder",template: "@{2}", newBody: "itemPlaceHolder");

            var actualString = htmlNode.WriteTo();
            var expectedString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <LayoutTemplate>
            <table class=""table"">
                <tbody>
                    @itemPlaceHolder
                    <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder1""></asp:PlaceHolder>
                    <asp:PlaceHolder runat=""server"" ID=""itemPlaceHolder2""></asp:PlaceHolder>
                </tbody>
            </table>
        </LayoutTemplate>
    </asp:ListView>
</div>";
            
            Assert.True(multipleNameMatchSingleIdMatch);
            Assert.AreEqual(expectedString, actualString);   
        }
        
        [Test]
        public void ConvertEmbeddedCode_Converts_DataBinding()
        {
            string htmlString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <ItemTemplate>
            <tr>
                <td>
                    <image class=""esh-thumbnail"" src='/Pics/<%#:Item.PictureFileName%>' />
                </td>
                <p>
                    <%#:Item.MaxStockThreshold%>
                </p>
                <td>
                    <asp:HyperLink NavigateUrl='<%# GetRouteUrl(""EditProductRoute"", new {id =Item.Id}) %>' runat=""server"" CssClass=""esh-table-link"">
                        Edit
                    </asp:HyperLink>
                </td>
            </tr>
        </ItemTemplate>
    </asp:ListView>
</div>";
            string contents = ControlConverter.ConvertEmbeddedCode(htmlString);

            string expectedContents = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <ItemTemplate>
            <tr>
                <td>
                    <image class=""esh-thumbnail"" src='/Pics/@(Item.PictureFileName)' />
                </td>
                <p>
                    @(Item.MaxStockThreshold)
                </p>
                <td>
                    <asp:HyperLink NavigateUrl='@(GetRouteUrl(""EditProductRoute"", new {id =Item.Id}) )' runat=""server"" CssClass=""esh-table-link"">
                        Edit
                    </asp:HyperLink>
                </td>
            </tr>
        </ItemTemplate>
    </asp:ListView>
</div>";
            Assert.AreEqual(expectedContents, contents);
        }
        
        [Test]
        public void ConvertEmbeddedCode_Converts_SingExpr()
        {
            string htmlString = @"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <span class=""esh-pager-item"">Showing <%: Model.ItemsPerPage%> of <%: Model.TotalItems%> products - Page <%: (Model.ActualPage + 1)%> - <%: Model.TotalPages%>
                </span>
            </nav>
        </article>
    </div>
</div>";
            string contents = ControlConverter.ConvertEmbeddedCode(htmlString);
            string expectedContents = @"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <span class=""esh-pager-item"">Showing @(Model.ItemsPerPage) of @(Model.TotalItems) products - Page @((Model.ActualPage + 1)) - @(Model.TotalPages)
                </span>
            </nav>
        </article>
    </div>
</div>";
            Assert.AreEqual(expectedContents, contents);
        }
    }
}