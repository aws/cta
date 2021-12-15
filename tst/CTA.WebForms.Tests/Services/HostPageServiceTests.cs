using CTA.WebForms.Services;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace CTA.WebForms.Tests.Services
{
    public class HostPageServiceTests
    {
        private const string TestTitle = "TestTitle";
        private const string TestNamespace = "TestNamespace";
        private const string TestStyleSheet1 = "Styles1.css";
        private const string TestStyleSheet2 = "Styles2.css";
        private string ExpectedPath => Path.Combine("Pages", "_Host.cshtml");
        private const string ExpectedNoStyleSheetContent =
@"@page ""/""
@namespace TestNamespace
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>TestTitle</title>
    <base href=""~/""/>

</head>
<body>
    <app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>

    <script src=""_framework/blazor.server.js""></script>
</body>
</html>";
        private const string ExpectedStyleSheetContent =
@"@page ""/""
@namespace TestNamespace
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>TestTitle</title>
    <base href=""~/""/>
    <link rel=""stylesheet"" href=""{0}"" />
</head>
<body>
    <app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>

    <script src=""_framework/blazor.server.js""></script>
</body>
</html>";

        private HostPageService _hostPageService;

        [SetUp]
        public void SetUp()
        {
            _hostPageService = new HostPageService();
            _hostPageService.Title = TestTitle;
            _hostPageService.HostNamespace = TestNamespace;
        }

        [Test]
        public void ConstructHostPageFile_Properly_Creates_File_Contents_Without_Stylesheets()
        {
            var fileBytes = _hostPageService.ConstructHostPageFile().FileBytes;
            var actualContent = Encoding.UTF8.GetString(fileBytes);

            Assert.AreEqual(ExpectedNoStyleSheetContent, actualContent);
        }

        public void AddStyleSheetPath_Result_In_File_With_Stylesheets()
        {
            _hostPageService.AddStyleSheetPath(TestStyleSheet1);
            _hostPageService.AddStyleSheetPath(TestStyleSheet2);

            var fileBytes = _hostPageService.ConstructHostPageFile().FileBytes;
            var actualContent = Encoding.UTF8.GetString(fileBytes);

            Assert.AreEqual(ExpectedNoStyleSheetContent, actualContent);
        }

        [Test]
        public void ConstructHostPageFile_Writes_To_Correct_Path()
        {
            var actualPath = _hostPageService.ConstructHostPageFile().RelativePath;

            Assert.AreEqual(ExpectedPath, actualPath);
        }
    }
}
