using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class ButtonControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>()
                {
                    ["onclick"] = "@onclick", 
                    ["cssclass"] = "class"
                };
            } 
        }
        protected override string BlazorName { get { return "button"; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var textAttr = node.Attributes.AttributesWithName("text").FirstOrDefault();
            var buttonText = textAttr?.Value ?? string.Empty;
            
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, GetNewAttributes(node.Attributes, NewAttributes), buttonText);
        }
    }
}
