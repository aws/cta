using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class HyperLinkControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get 
            { 
                return new Dictionary<string, string>()
                {
                    ["navigateurl"] = "href", 
                    ["cssclass"] = "class"
                };
                
            } 
        }
        protected override string BlazorName { get { return "a"; } }

        public HyperLinkControlConverter() : base()
        {
            
        }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            string newAttributes = "";   
            foreach (HtmlAttribute attr in node.Attributes)
            {
                if (AttributeMap.ContainsKey(attr.Name))
                {
                    string newAttr = AttributeMap[attr.Name] + "=" + attr.Value + " ";
                    newAttributes += newAttr;
                }
            }

            newAttributes = newAttributes.Trim();

            string template = 
                @"<{0} {1}>{2}</{0}>";
            
            string newContent = String.Format(template, BlazorName, newAttributes, node.InnerHtml);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            return newNode;
        }
    }
}
