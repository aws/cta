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
                    ["itemtype"] = "ItemType",
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

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            node.OwnerDocument.OptionOutputOriginalCase = true;

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
                    //Could accidentally replace placeholders for other stuff with different IDs,
                    //might need to add ID identifier as well
                    UpdateInnerHtmlNode(node, "asp:PlaceHolder", template: "@{2}", newBody: itemPlaceHolder);
                }
            }
            
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, GetNewAttributes(node.Attributes, NewAttributes), node.InnerHtml);
        }
    }
}
