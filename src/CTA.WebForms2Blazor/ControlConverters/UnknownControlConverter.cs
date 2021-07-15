using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class UnknownControlConverter : ControlConverter
    {
        protected override string NodeTemplate => "<div><!-- Migration of this control not supported <{0} {1}> -->{2}<!-- </{0}> --></div>";

        // No need to give meaningful values here, by definition we just don't know what
        // to populate them with and these will no longer be used due to our override of
        // the Convert2Blazor method
        protected override Dictionary<string, string> AttributeMap { get; }
        protected override string BlazorName { get; }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var attributeStrings = node.Attributes.Select(attr => GetAttributeAsString(attr));

            return Convert2BlazorFromParts(NodeTemplate, node.OriginalName, string.Join(' ', attributeStrings), node.InnerHtml);
        }
    }
}
