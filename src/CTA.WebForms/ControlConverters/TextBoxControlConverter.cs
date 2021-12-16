using System;
using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms.ControlConverters
{
    public class TextBoxControlConverter : ControlConverter
    {
        private const string MultiLineTextMode = "MultiLine";
        private const string PasswordTextMode = "Password";

        private const string MultiLineTagName = "textarea";

        private const string TextAttributeName = "text";
        private const string TextModeAttributeName = "textmode";
        private const string TypeAttributeName = "type";
        private const string ReadOnlyAttributeName = "readonly";
        private const string EnabledAttributeName = "enabled";
        private const string DisabledAttributeName = "disabled";
        private const string ValueAttributeName = "value";

        private const string TypeAttributePasswordValue = "password";

        protected override Dictionary<string, string> AttributeMap
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    ["id"] = "id",
                    ["cssclass"] = "class",
                    ["maxlength"] = "maxlength",
                    ["rows"] = "rows",
                    ["columns"] = "cols",
                    ["textchanged"] = "@onchange"
                };
            }
        }
        protected override string BlazorName { get { return "input"; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var tagName = BlazorName;
            var tagTemplate = SingleTagNodeTemplate;
            string tagBody = null;

            var textValue = node.Attributes.AttributesWithName(TextAttributeName).FirstOrDefault()?.Value;
            var textModeValue = node.Attributes.AttributesWithName(TextModeAttributeName).FirstOrDefault()?.Value
                ?? string.Empty;

            if (textModeValue.Equals(MultiLineTextMode, StringComparison.InvariantCultureIgnoreCase)) {
                tagName = MultiLineTagName;
                tagTemplate = NodeTemplate;
                tagBody = textValue;
            } else if (textModeValue.Equals(PasswordTextMode, StringComparison.InvariantCultureIgnoreCase)) {
                NewAttributes = NewAttributes.Append(new ViewLayerControlAttribute(TypeAttributeName, TypeAttributePasswordValue));
            }
            
            if (tagBody == null && textValue != null) {
                NewAttributes = NewAttributes.Append(new ViewLayerControlAttribute(ValueAttributeName, $"\"{textValue}\""));
            }

            AddBooleanAttributeOnCondition(node, ReadOnlyAttributeName, ReadOnlyAttributeName, true);
            AddBooleanAttributeOnCondition(node, EnabledAttributeName, DisabledAttributeName, false);

            var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);
            return Convert2BlazorFromParts(tagTemplate, tagName, joinedAttributesString, tagBody);
        }
    }
}
