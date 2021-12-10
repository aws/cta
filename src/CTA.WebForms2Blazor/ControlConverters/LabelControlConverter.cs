using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class LabelControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>()
                {
                    ["id"] = "id"
                };
            } 
        }
        protected override string BlazorName { get { return "label"; } }
        
        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var textAttr = node.Attributes.AttributesWithName("text").FirstOrDefault();
            var labelText = textAttr?.Value ?? string.Empty;

            var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, joinedAttributesString, labelText);
        }
    }
}
