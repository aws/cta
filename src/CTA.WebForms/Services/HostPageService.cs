using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CTA.WebForms.FileInformationModel;

namespace CTA.WebForms.Services
{
    public class HostPageService
    {
        private const string FileName = "_Host.cshtml";
        private const string HostTemplate =
@"@page ""/""
@namespace {0}
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{1}</title>
    <base href=""~/""/>
{2}
</head>
<body>
    <app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>

    <script src=""_framework/blazor.server.js""></script>
</body>
</html>";
        private const string StyleSheetTemplate = "    <link rel=\"stylesheet\" href=\"{0}\" />";

        public string Title { get; set; } = "Unknown_app_title";
        public string HostNamespace { get; set; } = "Unknown_host_namespace";

        private IEnumerable<string> _styleSheetPaths;

        public HostPageService()
        {
            _styleSheetPaths = new List<string>();
        }

        public void AddStyleSheetPath(string path)
        {
            _styleSheetPaths = _styleSheetPaths.Append(path);
        }

        public FileInformation ConstructHostPageFile()
        {
            var relativePath = Path.Combine(Constants.RazorPageDirectoryName, FileName);

            var styleSheetsString = ConstructStyleSheetsString();
            var fileContents = string.Format(HostTemplate, HostNamespace, Title, styleSheetsString);
            var fileBytes = Encoding.UTF8.GetBytes(fileContents);

            return new FileInformation(relativePath, fileBytes);
        }

        private string ConstructStyleSheetsString()
        {
            var styleSheetLines = _styleSheetPaths.Select(path => string.Format(StyleSheetTemplate, path));
            return string.Join(Environment.NewLine, styleSheetLines);
        }
    }
}
