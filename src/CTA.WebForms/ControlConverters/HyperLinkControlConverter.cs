using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms.ControlConverters
{
    public class HyperLinkControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get 
            { 
                return new Dictionary<string, string>()
                {
                    ["id"] = "id",
                    ["navigateurl"] = "href", 
                    ["cssclass"] = "class"
                };
                
            } 
        }
        protected override string BlazorName { get { return "a"; } }

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
