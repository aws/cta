using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using CTA.WebForms2Blazor.Services;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public abstract class ControlConverter
    {
        protected abstract Dictionary<string, string> AttributeMap { get; }
        protected virtual IEnumerable<string> NewAttributes
        {
            get { return null; }
        }
        protected abstract string BlazorName { get; }
        protected virtual string NodeTemplate { get { return @"<{0} {1}>{2}</{0}>"; } }
        
        public virtual HtmlNode Convert2Blazor(HtmlNode node)
        {
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, GetNewAttributes(node.Attributes, NewAttributes), node.InnerHtml);
        }

        protected virtual string GetNewAttributes(HtmlAttributeCollection oldAttributes,
            IEnumerable<string> additionalAttributes)
        {
            additionalAttributes ??= new List<string>();
            var convertedAttributes = ConvertAttributes(oldAttributes);
            var combinedAttributes = convertedAttributes.Concat(additionalAttributes);
            
            var combinedAttributesString = string.Join(" ", combinedAttributes);
            return combinedAttributesString;
        }

        protected IEnumerable<string> ConvertAttributes(HtmlAttributeCollection attributeCollection)
        {
            var convertedAttributes = attributeCollection
                .Where(attr => AttributeMap.ContainsKey(attr.Name))
                .Select(attr =>
                {
                    if (attr.QuoteType == AttributeValueQuote.DoubleQuote)
                    {
                        return $"{AttributeMap[attr.Name]}=\"{attr.Value}\"";
                    }
                    return $"{AttributeMap[attr.Name]}='{attr.Value}'";
                });
            
            return convertedAttributes;
        }

        protected string GetAttributeAsString(HtmlAttribute attr)
        {
            if (attr.QuoteType == AttributeValueQuote.DoubleQuote)
            {
                return $"{attr.OriginalName}=\"{attr.Value}\"";
            }

            return $"{attr.OriginalName}='{attr.Value}'";
        }

        protected HtmlNode Convert2BlazorFromParts(string template, string name, string attributes, string body)
        {
            string newContent = string.Format(template, name, attributes, body);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            return newNode;
        }
        
        public static string ConvertEmbeddedCode(string htmlString, ViewImportService viewImportService)
        {
            htmlString = EmbeddedCodeReplacers.ReplaceOneWayDataBinds(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceRawExprs(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceHTMLEncodedExprs(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceAspExprs(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceAspComments(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceDirectives(htmlString, viewImportService);

            return htmlString;
        }
    }
}
