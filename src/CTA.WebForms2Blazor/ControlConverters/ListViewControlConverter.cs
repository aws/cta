using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class ListViewControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>()
                {
                    ["id"] = "@ref",
                    ["itemtype"] = "itemtype",
                };
            } 
        }

        protected override IEnumerable<String> NewAttributes
        {
            get
            {
                return new List<String>() {"Context=Item"};
            }
        }
        
        protected override string BlazorName { get { return "ListView"; } }
        
        public ListViewControlConverter() : base()
        {
            
        }
        
        //This function only updates the first child node that matches the name,
        //but for current purposes there should only be one matching node
        private bool UpdateInnerHtmlNode(HtmlNode node, string name, 
            string template = null, 
            string newName = null, 
            IEnumerable<String> newAttributes = null, 
            string newBody = null)
        {
            var lowerName = name.ToLower();

            //Ideally use .SelectSingleNode, but doesnt work with names with ':' character and is less flexible
            //var selectedNode = node.SelectSingleNode(lowerName);
            
            var selectedNodes = node.Descendants().Where(child =>
            {
                return child.Name.ToLower() == lowerName || child.Id.ToLower() == lowerName;
            });
            var selectedNode = selectedNodes.FirstOrDefault();

            if (selectedNode != null)
            {
                template??= NodeTemplate;
                newName??= name;
                newAttributes??= new List<String>();
                newBody ??= selectedNode.InnerHtml;
                
                var parent = selectedNode.ParentNode;
                var newNode = Convert2BlazorFromParts(template, newName, 
                    GetNewAttributes(selectedNode.Attributes, newAttributes), newBody);
                parent.ReplaceChild(newNode, selectedNode);
                return true;
            }

            return false;
        }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var itemPlaceHolderIdAttr = node.Attributes.AttributesWithName("itemplaceholderid").FirstOrDefault();
            var itemPlaceHolder = itemPlaceHolderIdAttr?.Value ?? string.Empty;

            if (!itemPlaceHolder.Equals(string.Empty))
            {
                IEnumerable<String> layoutContextAttr = new List<String>()
                {
                    "Context=" + itemPlaceHolder
                };
            
                var success = UpdateInnerHtmlNode(node, "LayoutTemplate", newAttributes: layoutContextAttr);
                if (success)
                {
                    UpdateInnerHtmlNode(node, itemPlaceHolder, template: "@{2}", newBody: itemPlaceHolder);
                }
            }
            
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, GetNewAttributes(node.Attributes, NewAttributes), node.InnerHtml);
        }
    }
}
