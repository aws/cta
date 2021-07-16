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
    }
}