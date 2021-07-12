using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public abstract class ControlConverter
    {
        protected abstract Dictionary<String, String> AttributeMap { get; }
        protected virtual IEnumerable<String> NewAttributes
        {
            get { return null; }
        }
        protected abstract string BlazorName { get; }
        protected virtual string NodeTemplate { get { return @"<{0} {1}>{2}</{0}>"; } }

        protected ControlConverter()
        {
            //Constructor might not be needed
        }
        
        public virtual HtmlNode Convert2Blazor(HtmlNode node)
        {
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, GetNewAttributes(node.Attributes, NewAttributes), node.InnerHtml);
        }

        protected virtual string GetNewAttributes(HtmlAttributeCollection oldAttributes,
            IEnumerable<String> additionalAttributes)
        {
            additionalAttributes ??= new List<String>();
            var convertedAttributes = ConvertAttributes(oldAttributes);
            var combinedAttributes = convertedAttributes.Concat(additionalAttributes);
            
            var combinedAttributesString = string.Join(" ", combinedAttributes);
            return combinedAttributesString;
        }

        protected IEnumerable<String> ConvertAttributes(HtmlAttributeCollection attributeCollection)
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

        protected HtmlNode Convert2BlazorFromParts(string template, string name, string attributes, string body)
        {
            string newContent = String.Format(template, name, attributes, body);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            return newNode;
        }
        
        public static string ConvertEmbeddedCode(string htmlString)
        {
            // var documentNode = document;
            // var htmlString = documentNode.WriteTo();

            MatchEvaluator dataBindEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceDataBind);
            MatchEvaluator singleExprEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceSingleExpr);
            MatchEvaluator directiveEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceDirective);
            MatchEvaluator aspExprEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceAspExpr);
            
            htmlString = EmbeddedCodeReplacers.DataBindRegex.Replace(htmlString, dataBindEval);
            htmlString = EmbeddedCodeReplacers.SingleExpRegex.Replace(htmlString, singleExprEval);
            
            //Not implemented/used yet
            //htmlString = EmbeddedCodeReplacers.DirectiveRegex.Replace(htmlString, directiveEval);
            //htmlString = EmbeddedCodeReplacers.AspExpRegex.Replace(htmlString, aspExprEval);

            // var res = new HtmlDocument();
            // res.LoadHtml(htmlString);
            return htmlString;
        }
    }
}
