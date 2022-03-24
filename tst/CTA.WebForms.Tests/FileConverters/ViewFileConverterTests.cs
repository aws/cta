using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms.FileConverters;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CTA.WebForms.Tests.FileConverters
{
    [TestFixture]
    public class ViewFileConverterTests : WebFormsTestBase
    {
        private SemanticModel _testSemanticModel;
        private ClassDeclarationSyntax _testClassDeclaration;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var testTree = CSharpSyntaxTree.ParseText("namespace Test { public class TestClass { } }");
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var testCompilation = CSharpCompilation.Create(
                "Test",
                syntaxTrees: new[] { testTree },
                references: new[] { mscorlib });

            _testSemanticModel = testCompilation.GetSemanticModel(testTree);
            _testClassDeclaration = testTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        [Test]
        public async Task HyperLinkControlConverter_Returns_Href_Node()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestHyperLinkControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestHyperLinkControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "HyperLinkControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents =
@"<div class=""esh-pager"">
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
                <a class=""esh-table-link"" href=""@(GetRouteUrl('EditProductRoute', new {id =Item.Id}))"">
                    Edit
                </a>
                |
                <a class=""esh-table-link"" href=""@(GetRouteUrl('ProductDetailsRoute', new {id =Item.Id}))"">
                    Details
                </a>
                |
                <a class=""esh-table-link"" href=""@(GetRouteUrl('DeleteProductRoute', new {id =Item.Id}))"">
                    Delete
                </a>
            </td>
        </article>
    </div>
</div>";

            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task ButtonControlConverter_Returns_Button_Node()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestButtonControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestButtonControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ButtonControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents = 
@"<div class=""row"">
    <dl class=""col-md-6 dl-horizontal"">
        <dd class=""text-right esh-button-actions"">
            <a runat=""server"" href=""~"" class=""btn esh-button esh-button-secondary"">
                [ Cancel ]
            </a>
            <button class=""btn esh-button esh-button-primary"" @onclick=""(args) => Delete_Click(null, args)"">
                [ Delete ]
            </button>
            <button class=""btn test no-text"" @onclick=""(args) => Do_Something(null, args)""></button>
        </dd>
    </dl>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task LabelControlConverter_Returns_DynamicText()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestLabelControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestLabelControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "LabelControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents =
@"<div class=""row"">
    <dl class=""col-md-6 dl-horizontal"">
        <dt>
            Name
        </dt>
        <dd>
            <label>
                @(product.Name)
            </label>
        </dd>
        <dt>
            Description
        </dt>
        <dd>
            <label>
                @(product.Description)
            </label>
        </dd>
        <dt>
            Brand
        </dt>
        <dd>
            <label>
                @(product.CatalogBrand.Brand)
            </label>
        </dd>
        <dt>
            Type
        </dt>
        <dd>
            <label>
                @(product.CatalogType.Type)
            </label>
        </dd>
        <dt>
            Price
        </dt>
        <dd>
            <label class=""esh-price"">
                @(product.Price)
            </label>
        </dd>
        <dt>
            Picture name
        </dt>
        <dd>
            <label>
                @(product.PictureFileName)
            </label>
        </dd>
        <dt>
            Stock
        </dt>
        <dd>
            <label>
                @(product.AvailableStock)
            </label>
        </dd>
        <dt>
            Restock
        </dt>
        <dd>
            <label></label>
        </dd>
        <dt>
            Max stock
        </dt>
        <dd>
            <label id=""label1"">
                @(product.MaxStockThreshold)
            </label>
        </dd>
    </dl>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task ListViewControlConverter_Returns_ListView_Node()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestListViewControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestListViewControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ListViewControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =
@"<div class=""esh-table"">
    <ListView ID=""productList"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"" ItemPlaceholderID=""itemPlaceHolder"">
        <EmptyDataTemplate>
            <table>
                <tr>
                    <td>
                        No data was returned.
                    </td>
                </tr>
            </table>
        </EmptyDataTemplate>
        <LayoutTemplate>
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
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task TestViewFileConverter_Returns_GridView_Node()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestGridViewControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestGridViewControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "GridViewControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =
@"<div>
    <GridView ID=""GridView1"" AutoGenerateColumns=""False"" ItemType=""People"">
        <Columns>
            <BoundField DataField=""Name"" HeaderText=""First Name""></BoundField>
            <BoundField DataField=""LastName"" HeaderText=""Last Name""></BoundField>
            <BoundField DataField=""Position"" HeaderText=""Person Type""></BoundField>
        </Columns>
    </GridView>
</div>
<div>
    <GridView AutoGenerateColumns=""False"" DataKeyNames=""CustomerID"" EmptyDataText=""No data available"">
        <Columns>
            <BoundField DataField=""CustomerID"" HeaderText=""ID""></BoundField>
            <BoundField DataField=""CompanyName"" HeaderText=""CompanyName""></BoundField>
            <BoundField DataField=""FirstName"" HeaderText=""FirstName""></BoundField>
            <BoundField DataField=""LastName"" HeaderText=""LastName""></BoundField>
            <TemplateField>
                <ItemTemplate>
                    <button type=""button"">
                        Click Me! @(Item.Name)
                    </button>
                </ItemTemplate>
            </TemplateField>
            <ButtonField ButtonType=""Button"" CommandName=""Customer"" DataTextField=""CompanyName"" DataTextFormatString=""{0}""></ButtonField>
        </Columns>
    </GridView>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(fi.RelativePath, relativePath);
        }

        [Test]
        public async Task TestViewFileConverter_Returns_ContentPlaceHolderNode_As_Body_Directive()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestContentPlaceHolderControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestContentPlaceHolderControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ContentPlaceHolderControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =
@"<div>
    @Body
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        [Test]
        public async Task TestViewFileConverter_Returns_ContentNode_As_Div()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestContentControlFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestContentControlFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ContentControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =
@"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <SomeMadeUpTag class=""stuff""></SomeMadeUpTag>
            </nav>
        </article>
    </div>
</div>
<div>
    <p>
        Some random stuff
    </p>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }
        
        [Test]
        public async Task TestViewFileConverter_Converts_Directives()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestDirectiveFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestDirectiveFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "DirectiveOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);
            relativePath = Path.Combine("Pages", relativePath);

            string expectedContents =
@"@using CTA.WebForms.Tests.CustomControls
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
    @* The following tag is not supported: <TCounter:Counter ID=""counter1"" runat=""server"" IncrementAmount=""10""> *@@* </TCounter:Counter> *@
    @* The following tag is not supported: <Tfooter:Footer ID=""footer1"" runat=""server""> *@@* </Tfooter:Footer> *@
</div>";

            Assert.AreEqual(expectedContents, fileContents);
            Assert.AreEqual(relativePath, fi.RelativePath);
        }

        // This is a full view layer migration test; any features added later may cause this test to fail
        // and thus expectedContents might need to be updated in order for the test to be accurate
        [Test]
        public async Task TestViewFileConverter_DefaultAspx()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestViewFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestViewFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);

            var expectedContents =
@"@page ""/TestingArea/TestFiles/SampleViewFile""
@layout Site
@inherits eShopLegacyWebForms._Default
<!-- Conversion of Title attribute (value: ""Home Page"") for Page directive not currently supported -->
<!-- Conversion of AutoEventWireup attribute (value: ""true"") for Page directive not currently supported -->
@using CTA.WebForms.Tests
@using CTA.WebForms.Tests
<div>
    <GridView ID=""GridView1"" AutoGenerateColumns=""False"">
        <Columns>
            <BoundField DataField=""Name"" HeaderText=""First Name""></BoundField>
            <BoundField DataField=""LastName"" HeaderText=""Last Name""></BoundField>
            <BoundField DataField=""Position"" HeaderText=""Person Type""></BoundField>
        </Columns>
    </GridView>
</div>
<div class=""esh-table"">
    <p class=""esh-link-wrapper"">
        <a runat=""server"" href=""@(RouteUrl.RouteName=CreateProductRoute)"" class=""btn esh-button esh-button-primary"">
            Create New
        </a>
    </p>
    <ListView ID=""productList"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"" ItemPlaceholderID=""itemPlaceHolder"">
        <EmptyDataTemplate>
            <table>
                <tr>
                    <td>
                        No data was returned.
                    </td>
                </tr>
            </table>
        </EmptyDataTemplate>
        <LayoutTemplate>
            <table class=""table"">
                <thead>
                    <tr class=""esh-table-header"">
                        <th></th>
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
                    <image class=""esh-thumbnail"" src='/Pics/@(Item.PictureFileName)'></image>
                </td>
                <td>
                    <p>
                        @(Item.Name)
                    </p>
                </td>
                <td>
                    <a class=""esh-table-link"" href=""@(GetRouteUrl('EditProductRoute', new {id =Item.Id}))"">
                        Edit
                    </a>
                </td>
            </tr>
        </ItemTemplate>
    </ListView>
</div>
<div class=""esh-pager"">
    @* The following tag is not supported: <TCounter:Counter ID=""counter1"" runat=""server"" IncrementAmount=""10""> *@@* </TCounter:Counter> *@
    @* The following tag is not supported: <Tfooter:footer ID=""footer1"" runat=""server""> *@@* </Tfooter:footer> *@
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
        }

        [Test]
        public async Task TestViewFileConverter_SiteMaster()
        {
            var cbLinkerService = new CodeBehindReferenceLinkerService();

            FileConverter fc = new ViewFileConverter(
                FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestSiteMasterFilePath,
                new ViewImportService(),
                cbLinkerService,
                new TaskManagerService(),
                new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath),
                new WebFormMetricContext());

            cbLinkerService.RegisterClassDeclaration(
                FileConverterSetupFixture.TestSiteMasterFilePath,
                _testSemanticModel,
                _testClassDeclaration);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
        }

        [Test]
        public void ConvertEmbeddedCode_Converts_DataBinding()
        {
            string htmlString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <ItemTemplate>
            <tr>
                <td>
                    <image class=""esh-thumbnail"" src='/Pics/<%#Item.PictureFileName%>' />
                </td>
                <p>
                    <%#Item.MaxStockThreshold%>
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
            string contents = ViewFileConverter.ConvertEmbeddedCode(htmlString);

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
                    <asp:HyperLink NavigateUrl='@(GetRouteUrl(""EditProductRoute"", new {id =Item.Id}))' runat=""server"" CssClass=""esh-table-link"">
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
            string contents = ViewFileConverter.ConvertEmbeddedCode(htmlString);
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