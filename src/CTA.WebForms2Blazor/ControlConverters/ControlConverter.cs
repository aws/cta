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

        protected string ConvertAttributes(HtmlAttributeCollection attributeCollection)
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

            var convertedAttributesString = string.Join(" ", convertedAttributes);
            
            return convertedAttributesString;
        }

        protected HtmlNode Convert2BlazorFromParts(string template, string name, string attributes, string body)
        {
            string newContent = String.Format(template, name, attributes, body);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            return newNode;
        }
        
        public static HtmlDocument ConvertEmbeddedCode(HtmlDocument document)
        {
            var documentNode = document.DocumentNode;
            var htmlString = documentNode.WriteTo();

            MatchEvaluator dataBindEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceDataBind);
            MatchEvaluator singleExprEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceSingleExpr);
            MatchEvaluator directiveEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceDirective);
            MatchEvaluator aspExprEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceAspExpr);
            
            htmlString = EmbeddedCodeReplacers.DataBindRegex.Replace(htmlString, dataBindEval);
            htmlString = EmbeddedCodeReplacers.SingleExpRegex.Replace(htmlString, singleExprEval);
            htmlString = EmbeddedCodeReplacers.DirectiveRegex.Replace(htmlString, directiveEval);

            //Not implemented/used yet
            //htmlString = EmbeddedCodeReplacers.AspExpRegex.Replace(htmlString, aspExprEval);

            var res = new HtmlDocument();
            res.LoadHtml(htmlString);
            return res;
        }
    }
}
