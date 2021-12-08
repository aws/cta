using System;
using System.Collections.Generic;
using System.Linq;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class RadioButtonControlConverter : ControlConverter
    {
        private const string ContainerTagName = "div";
        private const string LabelTagName = "label";

        private const string IdentifierAttributeName = "id";
        private const string TextAttributeName = "text";
        private const string CheckedAttributeName = "checked";
        private const string EnabledAttributeName = "enabled";
        private const string DisabledAttributeName = "disabled";

        private const string LabelForAttributeTemplate = "for=\"{0}\"";

        protected override Dictionary<string, string> AttributeMap
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    ["id"] = "id",
                    ["cssclass"] = "class",
                    ["groupname"] = "name",
                };
            }
        }

        protected override IEnumerable<ViewLayerControlAttribute> NewAttributes { get; set; }
            = new[] { new ViewLayerControlAttribute("type", "radio") };

        protected override string BlazorName { get { return "input"; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var textValue = node.Attributes.AttributesWithName(TextAttributeName).FirstOrDefault()?.Value;
            var idValue = node.Attributes.AttributesWithName(IdentifierAttributeName).FirstOrDefault()?.Value;

            AddBooleanAttributeOnCondition(node, CheckedAttributeName, CheckedAttributeName, true);
            AddBooleanAttributeOnCondition(node, EnabledAttributeName, DisabledAttributeName, false);

            if (!string.IsNullOrEmpty(textValue)) {
                var containerNode = Convert2BlazorFromParts(NodeTemplate, ContainerTagName, null, null);

                // NOTE: If no is present, can't link label to radio button, so instead generate a new unique id
                if (idValue == null) {
                    idValue = IncrementalViewIdGenerator.GetNewGeneratedId();
                    NewAttributes = NewAttributes.Append(new ViewLayerControlAttribute(IdentifierAttributeName, idValue));
                }

                var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);
                var radioButtonNode = Convert2BlazorFromParts(SingleTagNodeTemplate, BlazorName, joinedAttributesString, null);

                var labelAttrString = string.Format(LabelForAttributeTemplate, idValue);
                var labelNode = Convert2BlazorFromParts(NodeTemplate, LabelTagName, labelAttrString, textValue);

                containerNode.AppendChild(radioButtonNode);
                containerNode.AppendChild(labelNode);

                return containerNode;
            } else {
                var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);
                return Convert2BlazorFromParts(SingleTagNodeTemplate, BlazorName, joinedAttributesString, null);
            }
        }
    }
}
