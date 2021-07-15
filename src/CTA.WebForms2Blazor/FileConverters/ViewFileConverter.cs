﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using CTA.WebForms2Blazor.Services;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ViewFileConverter : FileConverter
    {
        private ViewImportService _viewImportService;
        private List<ControlConversionAction> ControlActions;

        public ViewFileConverter(string sourceProjectPath, string fullPath, ViewImportService viewImportService) 
            : base(sourceProjectPath, fullPath)
        {
            _viewImportService = viewImportService;
            ControlActions = new List<ControlConversionAction>();
        }

        private HtmlDocument GetRazorContents()
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(FullPath);
            
            
            FindConversionActions(htmlDoc.DocumentNode, null);
            
            // This will modify the HtmlDocument nodes that will then be changed to a file information object
            ConvertNodes();
            
            
            return htmlDoc;
        }

        // Performs a DFS traversal of the HTML tree, adding nodes to be converted in postorder
        private void FindConversionActions(HtmlNode node, HtmlNode parent)
        {
            if (node == null)
            {
                return;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                FindConversionActions(child, node);
            }

            GetActions(node, parent);
        }

        private void GetActions(HtmlNode node, HtmlNode parent)
        {
            if (SupportedControls.ControlRulesMap.ContainsKey(node.Name))
            {
                var conversionAction = new ControlConversionAction(node, parent, SupportedControls.ControlRulesMap[node.Name]);
                ControlActions.Add(conversionAction);
            }
            else if (SupportedControls.ControlNameFormatRegex.IsMatch(node.Name))
            {
                // If the node appears to be a web forms control but we have no defined control
                // converter in the ControlRulesMap, then add action to comment it out as a default
                ControlActions.Add(new ControlConversionAction(node, parent, SupportedControls.DefaultControlConverter));
            }
        }

        private void ConvertNodes()
        {
            foreach (var package in ControlActions)
            {
                HtmlNode convertedNode = package.Rules.Convert2Blazor(package.Node);
                package.Parent.ReplaceChild(convertedNode, package.Node);
            }
        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();

            // View file converters will return razor file contents with
            // only view layer, code behind will be created in another file

            HtmlDocument migratedDocument = GetRazorContents();
            string contents = migratedDocument.DocumentNode.WriteTo();
            contents = ControlConverter.ConvertEmbeddedCode(contents, _viewImportService);

            // Currently just changing extension to .razor, keeping filename and directory the same
            // but Razor files are renamed and moved around, can't always use same filename/directory in the future
            string newRelativePath = FilePathHelper.AlterFileName(RelativePath, newExtension: ".razor");

            if (RelativePath.EndsWith(Constants.WebFormsPageMarkupFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                newRelativePath = Path.Combine(Constants.RazorPageDirectoryName, newRelativePath);
            }
            else if (RelativePath.EndsWith(Constants.WebFormsMasterPageMarkupFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                newRelativePath = Path.Combine(Constants.RazorLayoutDirectoryName, newRelativePath);
            }
            else if (RelativePath.EndsWith(Constants.WebFormsControlMarkupFileExtenion, StringComparison.InvariantCultureIgnoreCase))
            {
                newRelativePath = Path.Combine(Constants.RazorComponentDirectoryName, newRelativePath);
            }
            else
            {
                LogEnd();

                // Stuff like Global.asax shouldn't create a Global.razor file
                return Enumerable.Empty<FileInformation>();
            }

            LogEnd();

            return new List<FileInformation>() { new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(contents)) };
        }
    }
}