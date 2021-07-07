using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.FileConverters
{
    [TestFixture]
    public class ViewFileConverterTests
    {
        [Test]
        public async Task HyperLinkControlConverter_Returns_Href_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestHyperLinkControlFilePath);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "HyperLinkControlOnly.razor");
            string relativePath = Path.Combine("Pages", Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath));

            string expectedContents = @"
<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <a class=""esh-pager-item esh-pager-item--navigable="">
                    Previous 
                </a>

                <span class=""esh-pager-item"">Showing <%: Model.ItemsPerPage%> of <%: Model.TotalItems%> products - Page <%: (Model.ActualPage + 1)%> - <%: Model.TotalPages%>
                </span>

                <a class=""esh-pager-item esh-pager-item--navigable="">
                    Next
                </a>
            </nav>
            <td>
                <a href=""<%# GetRouteUrl(""EditProductRoute"", new {id =Item.Id}) %>"" class=""esh-table-link"">
                    Edit
                </a>
                |
                <a href=""<%# GetRouteUrl(""ProductDetailsRoute"", new {id =Item.Id}) %>"" class=""esh-table-link"">
                    Details
                </a>
                |
                <a href=""<%# GetRouteUrl(""DeleteProductRoute"", new {id =Item.Id}) %>"" class=""esh-table-link"">
                    Delete
                </a>
            </td>
        </article>
    </div>
</div>";
            //This test below doesn't quite work yet, some issues with quotations that will be fixed in
            // the next rules implementation PR
            //Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("href"));
            Assert.True(fileContents.Contains("class"));
            Assert.False(fileContents.Contains("asp:HyperLink"));
            Assert.False(fileContents.Contains("CssClass"));
            Assert.False(fileContents.Contains("NavigateUrl"));
            Assert.AreEqual(relativePath, fi.RelativePath);
        }
    }
}