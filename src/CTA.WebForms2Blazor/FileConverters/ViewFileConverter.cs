using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ViewFileConverter : FileConverter
    {
        private Dictionary<String, ControlConverter> ControlRulesMap; //From constructor or service
        //Tuple contains node to be converted, parent node, and instructions to convert node in that order
        private List<(HtmlNode Node, HtmlNode Parent, ControlConverter Rules)> NodesToBeConverted;
        private string _relativeDirectory;

        public ViewFileConverter(string sourceProjectPath, string fullPath, Dictionary<String, ControlConverter> dict) 
            : base(sourceProjectPath, fullPath)
        {
            // TODO: Register file with necessary services
            _relativeDirectory = Path.GetDirectoryName(RelativePath);
            NodesToBeConverted = new List<(HtmlNode Node, HtmlNode Parent, ControlConverter Rules)>();
            ControlRulesMap = dict;
        }

        private HtmlDocument GetRazorContents()
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(FullPath);
            DfsPostorderWalker(htmlDoc.DocumentNode, null);
            
            //This will either return string contents or modified HtmlDocument
            //that will then be changed to a file information object
            
            ConvertNodes();
            return htmlDoc;
        }

        private void DfsPostorderWalker(HtmlNode node, HtmlNode parent)
        {
            if (node == null)
            {
                return;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                DfsPostorderWalker(child, node);
            }

            GetRules(node, parent);
        }

        private void GetRules(HtmlNode node, HtmlNode parent)
        {
            if (ControlRulesMap.ContainsKey(node.Name))
            {
                var conversionTuple = (node, parent, ControlRulesMap[node.Name]);
                NodesToBeConverted.Add(conversionTuple);
            }
        }

        private void ConvertNodes()
        {
            foreach (var package in NodesToBeConverted)
            {
                HtmlNode convertedNode = package.Rules.Convert2Blazor(package.Node);
                package.Parent.ReplaceChild(convertedNode, package.Node);
            }
        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            // TODO: Store UI information in necessary services

            // View file converters will return razor file contents with
            // only view layer, code behind will be created in another file
            HtmlDocument migratedDocument = GetRazorContents();
            string contents = migratedDocument.DocumentNode.WriteTo();

            string newFileName = "testing.razor"; //new file name depends, add later
            string newPath = Path.Combine(_relativeDirectory, newFileName);
            
            var fileList = new List<FileInformation>() {new FileInformation(newPath, Encoding.UTF8.GetBytes(contents))};

            return fileList;
        }
    }
}
