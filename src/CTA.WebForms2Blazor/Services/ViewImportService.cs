using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.Services
{
    public class ViewImportService
    {
        private const string UsingDirectiveStart = "@using";
        private const string InvalidUsingDirectiveFormatErrorTemplate = "Attempted to add using directive \"{0}\" but statement does not match \"@using NamespaceName\" format";
        private const string ImportsFileName = "_Imports.razor";

        // TODO: In the future, storing the locations of files that use
        // certain usings could allow us to place multiple _Imports.razor
        // files, keeping using directives as low as possible in the file
        // hierarchy, may be unnecessarily complicated process for little
        // gain however

        // TODO: May want to determine shared folder layouts and add some
        // @layout directives to lower-level _Imports.razor files

        private readonly ISet<string> ViewUsingDirectives = new HashSet<string>()
        {
            // Initialize with set of basic usings likely to be required
            "@using BlazorWebFormsComponents",
            "@using Microsoft.AspNetCore.Authorization",
            "@using Microsoft.AspNetCore.Components.Forms",
            "@using Microsoft.AspNetCore.Components.Routing",
            "@using Microsoft.AspNetCore.Components.Web",
            "@using System.Net.Http"
        };

        public void AddViewImport(string usingDirective)
        {
            usingDirective = usingDirective.Trim();

            if (!usingDirective.StartsWith(UsingDirectiveStart))
            {
                throw new ArgumentException(string.Format(InvalidUsingDirectiveFormatErrorTemplate, usingDirective));
            }

            ViewUsingDirectives.Add(usingDirective);
        }

        public FileInformation ConstructImportsFile()
        {
            // Apply normal alphabetical ordering to set and add to separate lines
            var content = string.Join(Environment.NewLine, ViewUsingDirectives.OrderBy(usingDirective => usingDirective));

            // ImportsFileName is good enough for relative path as we want this file
            // to be in the project root directory
            return new FileInformation(ImportsFileName, Encoding.UTF8.GetBytes(content));
        }
    }
}
