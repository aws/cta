using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class HyperLinkControlConverter : ControlConverter
    {
        private Dictionary<String, String> _hyperLinkControlsMap = new Dictionary<string, string>() 
            {["navigateurl"] = "href", ["cssclass"] = "class"};
        private readonly string _name = "a";
        
        public HyperLinkControlConverter() : base()
        {
            
        }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            string newAttributes = "";   
            foreach (HtmlAttribute attr in node.Attributes)
            {
                if (_hyperLinkControlsMap.ContainsKey(attr.Name))
                {
                    string newAttr = _hyperLinkControlsMap[attr.Name] + "=" + attr.Value + " ";
                    newAttributes += newAttr;
                }
            }

            newAttributes = newAttributes.Trim();

            string template = 
                @"<{0} {1}>{2}</{0}>";
            
            string newContent = String.Format(template, _name, newAttributes, node.InnerHtml);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            return newNode;
        }
    }
}
