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
                    ["id"] = "for"
                };
            } 
        }
        protected override string BlazorName { get { return "label"; } }

        protected override string NodeTemplate { get { return @"{2}"; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var textAttr = node.Attributes.AttributesWithName("text").Single();
            var labelText = textAttr.Value;

            //Not sure if this needs to be handled and if this is expected behavior
            if (node.Attributes.Contains("id"))
            {
                return Convert2BlazorFromParts(base.NodeTemplate, BlazorName, ConvertAttributes(node.Attributes), labelText);
            }

            return Convert2BlazorFromParts(NodeTemplate, null, null, labelText);
        }
    }
}
