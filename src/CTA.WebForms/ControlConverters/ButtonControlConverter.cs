using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms.ControlConverters
{
    public class ButtonControlConverter : ControlConverter
    {
        private const string EnabledAttributeName = "enabled";
        private const string DisabledAttributeName = "disabled";

        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>()
                {
                    ["id"] = "id",
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

            AddBooleanAttributeOnCondition(node, EnabledAttributeName, DisabledAttributeName, false);
            var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);

            return Convert2BlazorFromParts(NodeTemplate, BlazorName, joinedAttributesString, buttonText);
        }
    }
}
