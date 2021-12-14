using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class UserControlConverter : ControlConverter
    {
        private readonly string _blazorName;
        private readonly HashSet<string> _removeAttributeSet;
        private readonly Dictionary<string, string> _attributeMap;
        
        protected override Dictionary<String, String> AttributeMap
        {
            get
            {
                return _attributeMap;
            }
        }
        public UserControlConverter(string blazorName)
        {
            _blazorName = blazorName;
            _attributeMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _removeAttributeSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "ID",
                "runat",
            };
        }
        protected override string BlazorName { get { return _blazorName; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            SetAttributeMap(node);
            var joinedAttributesString = JoinAllAttributes(node.Attributes, null);
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, joinedAttributesString, node.InnerHtml);
        }

        //Adds every attribute to the AttributeMap that is not in the removeAttributeSet
        private void SetAttributeMap(HtmlNode node)
        {
            foreach (HtmlAttribute attr in node.Attributes)
            {
                if (!_removeAttributeSet.Contains(attr.Name))
                {
                    _attributeMap.Add(attr.Name, attr.OriginalName);
                }
            }
        }
    }
}
