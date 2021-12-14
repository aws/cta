using System.Collections.Generic;
using System.Linq;
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
                    ["id"] = "id",
                    ["cssclass"] = "class"
                };
            } 
        }
        protected override string BlazorName { get { return "label"; } }
        
        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var labelText = node.InnerHtml;

            if (string.IsNullOrEmpty(node.InnerHtml))
            {
                var textAttr = node.Attributes.AttributesWithName("text").FirstOrDefault();
                labelText = textAttr?.Value ?? string.Empty;
            }

            var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, joinedAttributesString, labelText);
        }
    }
}
