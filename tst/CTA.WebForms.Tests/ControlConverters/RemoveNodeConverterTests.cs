using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ControlConverters
{
    public class RemoveNodeConverterTests
    {
        ControlConverter _testRemoveNodeKeepContentsConverter;
        private ControlConverter _testRemoveNodeAndContentsConverter;
        
        [SetUp]
        public void Setup()
        {
            _testRemoveNodeKeepContentsConverter = new RemoveNodeKeepContentsConverter();
            _testRemoveNodeAndContentsConverter = new RemoveNodeAndContentsConverter();
        }
        
        [Test]
        public void RemoveNodeKeepContentsConverter_Returns_ContentControl_Inner_Nodes()
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
            var htmlDoc = new HtmlDocument();
            htmlDoc.OptionOutputOriginalCase = true;
            htmlDoc.LoadHtml(htmlString);
            var htmlNode = htmlDoc.DocumentNode.FirstChild;
            var convertedNode = _testRemoveNodeKeepContentsConverter.Convert2Blazor(htmlNode);

            var actualString = htmlDoc.DocumentNode.WriteTo();
            var expectedString = @"
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
";
            
            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void RemoveNodeKeepContentsConverter_Removes_Body_Node()
        {
            var htmlString = @"<body>
    <form runat=""server"">
        <footer class=""esh-app-footer"">
            <div class=""container"">
                <article class=""row"">
                    
                </article>
            </div>
        </footer>
    </form>
</body>";
            var htmlDoc = new HtmlDocument();
            htmlDoc.OptionOutputOriginalCase = true;
            htmlDoc.LoadHtml(htmlString);
            var htmlNode = htmlDoc.DocumentNode.FirstChild;
            _testRemoveNodeKeepContentsConverter.Convert2Blazor(htmlNode);

            var actualString = htmlDoc.DocumentNode.WriteTo();
            var expectedString = @"
    <form runat=""server"">
        <footer class=""esh-app-footer"">
            <div class=""container"">
                <article class=""row"">
                    
                </article>
            </div>
        </footer>
    </form>
";
            
            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void RemoveNodeAndContentsConverter_Removes_Scripts_Node()
        {
            var htmlString = @"<form runat=""server"">
    <ScriptManager runat=""server"">
        <Scripts>
            <asp:ScriptReference Name=""MsAjaxBundle"" />
            <asp:ScriptReference Name=""jquery"" />
        </Scripts>
    </ScriptManager>
    <section class=""esh-app-hero"">
        <div class=""container esh-header"">
            <h1 class=""esh-header-title"">Catalog manager <span>(Web Forms)</span></h1>
        </div>
    </section>
</form>";
            var htmlDoc = new HtmlDocument();
            htmlDoc.OptionOutputOriginalCase = true;
            htmlDoc.LoadHtml(htmlString);
            var htmlNode = htmlDoc.DocumentNode.SelectSingleNode("//form/scriptmanager/scripts");
            _testRemoveNodeAndContentsConverter.Convert2Blazor(htmlNode);

            var actualString = htmlDoc.DocumentNode.WriteTo();
            var expectedString = @"<form runat=""server"">
    <ScriptManager runat=""server"">
        
    </ScriptManager>
    <section class=""esh-app-hero"">
        <div class=""container esh-header"">
            <h1 class=""esh-header-title"">Catalog manager <span>(Web Forms)</span></h1>
        </div>
    </section>
</form>";
            
            Assert.AreEqual(expectedString, actualString);
        }
        
    }
}