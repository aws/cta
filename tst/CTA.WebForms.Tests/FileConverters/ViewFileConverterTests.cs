using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms.ControlConverters;
using CTA.WebForms.FileConverters;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.FileConverters
{
    [TestFixture]
    public class ViewFileConverterTests
    {
        [Test]
        public async Task HyperLinkControlConverter_Returns_Href_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestHyperLinkControlFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "HyperLinkControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents = @"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <a id=""PaginationPrevious"" class=""esh-pager-item esh-pager-item--navigable"">
                    Previous
                </a>

                <a id=""PaginationNext"" class=""esh-pager-item esh-pager-item--navigable"">
                    Next
                </a>
            </nav>
            <td>
                <a href='@(GetRouteUrl(""EditProductRoute"", new {id =Item.Id}))' class=""esh-table-link"">
                    Edit
                </a>
                |
                <a href='@(GetRouteUrl(""ProductDetailsRoute"", new {id =Item.Id}))' class=""esh-table-link"">
                    Details
                </a>
                |
                <a href='@(GetRouteUrl(""DeleteProductRoute"", new {id =Item.Id}))' class=""esh-table-link"">
                    Delete
                </a>
            </td>
        </article>
    </div>
</div>";

            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("href"));
            Assert.True(fileContents.Contains("class"));
            Assert.False(fileContents.Contains("asp:HyperLink"));
            Assert.False(fileContents.Contains("CssClass"));
            Assert.False(fileContents.Contains("NavigateUrl"));
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task ButtonControlConverter_Returns_Button_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestButtonControlFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ButtonControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents = @"<div class=""row"">
    <dl class=""col-md-6 dl-horizontal"">
        <dd class=""text-right esh-button-actions"">
            <a runat=""server"" href=""~"" class=""btn esh-button esh-button-secondary"">[ Cancel ]
            </a>
            <button class=""btn esh-button esh-button-primary"" @onclick=""Delete_Click"">[ Delete ]</button>
            <button class=""btn test no-text"" @onclick=""Do_Something""></button>
        </dd>
    </dl>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("button"));
            Assert.True(fileContents.Contains("class"));
            Assert.True(fileContents.Contains("</button>"));
            Assert.False(fileContents.Contains("asp:Button"));
            Assert.False(fileContents.Contains("CssClass"));
            Assert.False(fileContents.Contains("Text"));
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task LabelControlConverter_Returns_DynamicText()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestLabelControlFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "LabelControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents = @"<div class=""row"">
    <dl class=""col-md-6 dl-horizontal"">
        <dt>Name
        </dt>

        <dd>
            <label>@(product.Name)</label>
        </dd>

        <dt>Description
        </dt>

        <dd>
            <label>@(product.Description)</label>
        </dd>

        <dt>Brand
        </dt>

        <dd>
            <label>@(product.CatalogBrand.Brand)</label>
        </dd>

        <dt>Type
        </dt>

        <dd>
            <label>@(product.CatalogType.Type)</label>
        </dd>
        <dt>Price
        </dt>

        <dd>
            <label class=""esh-price"">@(product.Price)</label>
        </dd>

        <dt>Picture name
        </dt>

        <dd>
            <label>@(product.PictureFileName)</label>
        </dd>

        <dt>Stock
        </dt>

        <dd>
            <label>@(product.AvailableStock)</label>
        </dd>

        <dt>Restock
        </dt>

        <dd>
            <label></label>
        </dd>

        <dt>Max stock
        </dt>

        <dd>
            <label id=""label1"">@(product.MaxStockThreshold)</label>
        </dd>

    </dl>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("<label"));
            Assert.True(fileContents.Contains("</label>"));
            Assert.False(fileContents.Contains("asp:Label"));
            Assert.False(fileContents.Contains("Text"));
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task ListViewControlConverter_Returns_ListView_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestListViewControlFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ListViewControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents = @"<div class=""esh-table"">
    <ListView @ref=""productList"" ItemPlaceholderID=""itemPlaceHolder"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"" Context=""Item"">
        <EmptyDataTemplate>
            <table>
                <tr>
                    <td>No data was returned.</td>
                </tr>
            </table>
        </EmptyDataTemplate>
        <LayoutTemplate Context=""itemPlaceHolder"">
            <table class=""table"">
                <thead>
                    <tr class=""esh-table-header"">
                        <th>
                            Name
                        </th>
                    </tr>
                </thead>
                <tbody>
                    @itemPlaceHolder
                </tbody>
            </table>
        </LayoutTemplate>
        <ItemTemplate>
            <tr>
                <td>
                    <p>
                        @(Item.Name)
                    </p>
                </td>
            </tr>
        </ItemTemplate>
    </ListView>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("ListView"));
            Assert.True(fileContents.Contains(@"Context=""itemPlaceHolder"""));
            Assert.False(fileContents.Contains("asp:ListView"));
            Assert.False(fileContents.Contains("asp:PlaceHolder"));
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task TestViewFileConverter_Returns_GridView_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestGridViewControlFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "GridViewControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents = @"<div>
    <GridView @ref=""GridView1"" AutoGenerateColumns=""false"" ItemType=""People"" SelectMethod=""People.GetPeople"">
        <Columns>
            <BoundField DataField=""Name"" HeaderText=""First Name"" ItemType=""Random""></BoundField>
            <BoundField DataField=""LastName"" HeaderText=""Last Name"" ItemType=""People""></BoundField>
            <BoundField DataField=""Position"" HeaderText=""Person Type"" ItemType=""People""></BoundField>
        </Columns>
    </GridView>
</div>
<div>
    <GridView AutoGenerateColumns=""false"" DataKeyNames=""CustomerID"" SelectMethod=""GetCustomers"" EmptyDataText=""No data available"" ItemType=""PleaseReplaceWithActualItemTypeHere"">
        <Columns>
            <BoundField DataField=""CustomerID"" HeaderText=""ID"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
            <BoundField DataField=""CompanyName"" HeaderText=""CompanyName"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
            <BoundField DataField=""FirstName"" HeaderText=""FirstName"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
            <BoundField DataField=""LastName"" HeaderText=""LastName"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
            <TemplateField ItemType=""PleaseReplaceWithActualItemTypeHere"">
                <ItemTemplate Context=""Item"">
                    <button type=""button"">Click Me! @(Item.Name)</button>
                </ItemTemplate>
            </TemplateField>
            <ButtonField ButtonType=""Button"" DataTextField=""CompanyName"" DataTextFormatString=""{0}"" CommandName=""Customer"" ItemType=""PleaseReplaceWithActualItemTypeHere""></ButtonField>
        </Columns>
    </GridView>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("GridView"));
            Assert.True(fileContents.Contains(@"ref=""GridView1"""));
            Assert.False(fileContents.Contains("asp:GridView"));
            Assert.False(fileContents.Contains("asp:BoundField"));
            Assert.AreEqual(fi.RelativePath, relativePath);
        }

        [Test]
        public async Task TestViewFileConverter_Returns_ContentPlaceHolderNode_As_Body_Directive()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestContentPlaceHolderControlFilePath,
                new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ContentPlaceHolderControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents = @"<div>
    @Body
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task TestViewFileConverter_Returns_ContentNode_As_Div()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestContentControlFilePath,
                new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ContentControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =  @"
    <div class=""esh-pager"">
        <div class=""container"">
            <article class=""esh-pager-wrapper row"">
                <nav>
                    <SomeMadeUpTag class=""stuff""></SomeMadeUpTag>
                </nav>
            </article>
        </div>
    </div>
    <div>
        <p> Some random stuff </p>
    </div>
";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }
        
        [Test]
        public async Task TestViewFileConverter_Converts_Directives()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestDirectiveFilePath,
                new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "DirectiveOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =  @"@using CTA.WebForms.Tests.CustomControls
@using eShopOnBlazor
<!-- Cannot convert file name to namespace, file path Footer.ascx does not have a directory -->
<!-- Register Src=""Footer.ascx"" TagName=""Footer1"" TagPrefix=""TFooter1"" -->
@page ""/TestingArea/TestFiles/DirectiveOnly""
@layout Site
@inherits eShopLegacyWebForms._Default
<!-- Conversion of Title attribute (value: ""Home Page"") for Page directive not currently supported -->
<!-- Conversion of AutoEventWireup attribute (value: ""true"") for Page directive not currently supported -->
@namespace Replace_this_with_code_behind_namespace
@inherits LayoutComponentBase
<!-- Conversion of autoeventwireup attribute (value: ""true"") for Master directive not currently supported -->
<!-- Conversion of inherits attribute (value: ""eShopLegacyWebForms.SiteMaster"") for Master directive not currently supported -->
<!-- Conversion of AutoEventWireup attribute (value: ""true"") for Control directive not currently supported -->

<div>
    <Counter IncrementAmount=""10""></Counter>
    <Foobar></Foobar>
</div>";

            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        // This is a full view layer migration test; any features added later may cause this test to fail
        // and thus expectedContents might need to be updated in order for the test to be accurate
        [Test]
        public async Task TestViewFileConverter_DefaultAspx()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestViewFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);

            var expectedContents = @"@page ""/TestingArea/TestFiles/SampleViewFile""
@layout Site
@inherits eShopLegacyWebForms._Default
<!-- Conversion of Title attribute (value: ""Home Page"") for Page directive not currently supported -->
<!-- Conversion of AutoEventWireup attribute (value: ""true"") for Page directive not currently supported -->
@using CTA.WebForms.Tests
@using CTA.WebForms.Tests


    <div>
        <GridView @ref=""GridView1"" AutoGenerateColumns=""false"" ItemType=""PleaseReplaceWithActualItemTypeHere"">
            <Columns>
                <BoundField DataField=""Name"" HeaderText=""First Name"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
                <BoundField DataField=""LastName"" HeaderText=""Last Name"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
                <BoundField DataField=""Position"" HeaderText=""Person Type"" ItemType=""PleaseReplaceWithActualItemTypeHere""></BoundField>
            </Columns>
        </GridView>
    </div>

    <div class=""esh-table"">
        <p class=""esh-link-wrapper"">
            <a runat=""server"" href=""@(RouteUrl.RouteName=CreateProductRoute)"" class=""btn esh-button esh-button-primary"">
                Create New
            </a>
        </p>

        <ListView @ref=""productList"" ItemPlaceholderID=""itemPlaceHolder"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"" Context=""Item"">
            <EmptyDataTemplate>
                <table>
                    <tr>
                        <td>No data was returned.</td>
                    </tr>
                </table>
            </EmptyDataTemplate>
            <LayoutTemplate Context=""itemPlaceHolder"">
                <table class=""table"">
                    <thead>
                        <tr class=""esh-table-header"">
                            <th></th>
                            <th>Name</th>
                        </tr>
                    </thead>
                    <tbody>
                        @itemPlaceHolder
                    </tbody>
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <image class=""esh-thumbnail"" src='/Pics/@(Item.PictureFileName)'></image>
                    </td>
                    <td>
                        <p>@(Item.Name)</p>
                    </td>
                    <td>
                        <a href='@(GetRouteUrl(""EditProductRoute"", new {id =Item.Id}))' class=""esh-table-link"">
                            Edit
                        </a>
                    </td>
                </tr>
            </ItemTemplate>
        </ListView>
    </div>

    <div class=""esh-pager"">
        <Counter IncrementAmount=""10""></Counter>
        <footer></footer>
    </div>

";
            
            Assert.AreEqual(expectedContents, fileContents);
        }

        [Test]
        public async Task TestViewFileConverter_SiteMaster()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestSiteMasterFilePath, new ViewImportService(), new TaskManagerService(), new WebFormMetricContext());
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
        }
    }
}