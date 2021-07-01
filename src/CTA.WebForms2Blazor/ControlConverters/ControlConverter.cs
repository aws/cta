using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public abstract class ControlConverter
    {
        protected abstract Dictionary<String, String> AttributeMap { get; }
        protected abstract string BlazorName { get; }
        protected virtual string NodeTemplate { get { return @"<{0} {1}>{2}</{0}>"; } }
        
        protected ControlConverter()
        {
            //Constructor might not be needed
        }
        
        public virtual HtmlNode Convert2Blazor(HtmlNode node)
        {
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, ConvertAttributes(node.Attributes), node.InnerHtml);
        }

        private string ConvertAttributes(HtmlAttributeCollection attributeCollection)
        {
            var convertedAttributes = attributeCollection
                .Where(attr => AttributeMap.ContainsKey(attr.Name))
                .Select(attr => $"{AttributeMap[attr.Name]}={attr.Value}");
            
            var convertedAttributesString = string.Join(" ", convertedAttributes);
            
            return convertedAttributesString;
        }

        private HtmlNode Convert2BlazorFromParts(string template, string name, string attributes, string body)
        {
            string newContent = String.Format(template, name, attributes, body);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            return newNode;
        }
    }
}
