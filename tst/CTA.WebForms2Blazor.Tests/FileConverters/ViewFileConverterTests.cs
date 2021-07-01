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
        public async Task TestViewFileConverter()
        {
            var controlMap = new Dictionary<string, ControlConverter>() {["asp:hyperlink"]=new HyperLinkControlConverter()};
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, FileConverterSetupFixture.TestViewFilePath, controlMap);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var projectFileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "testing.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);

            Assert.True(projectFileContents.Contains("href"));
            Assert.True(projectFileContents.Contains("class"));
            Assert.False(projectFileContents.Contains("asp:HyperLink"));
            Assert.False(projectFileContents.Contains("CssClass"));
            Assert.False(projectFileContents.Contains("NavigateUrl"));
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));
        }
    }
}