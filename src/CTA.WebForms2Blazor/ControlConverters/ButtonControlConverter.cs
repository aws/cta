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
            var textAttr = node.Attributes.AttributesWithName("text").Single();
            var buttonText = textAttr.Value;
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, ConvertAttributes(node.Attributes), buttonText);
        }
    }
}
