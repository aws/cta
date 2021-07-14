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
            node.OwnerDocument.OptionOutputOriginalCase = true;
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
        
        //private 

        protected HtmlNode Convert2BlazorFromParts(string template, string name, string attributes, string body)
        {
            string newContent = String.Format(template, name, attributes, body);
            HtmlNode newNode = HtmlNode.CreateNode(newContent);
            newNode.OwnerDocument.OptionOutputOriginalCase = true;
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
            htmlString = EmbeddedCodeReplacers.DirectiveRegex.Replace(htmlString, directiveEval);

            //Not implemented/used yet
            //htmlString = EmbeddedCodeReplacers.AspExpRegex.Replace(htmlString, aspExprEval);

            // var res = new HtmlDocument();
            // res.LoadHtml(htmlString);
            return htmlString;
        }
        
        //This function only updates the first child node that matches the name,
        //but for current purposes there should only be one matching node
        protected bool UpdateInnerHtmlNode(HtmlNode outerNode, string targetName, 
            string template = null, 
            string newName = null, 
            IEnumerable<String> newAttributes = null, 
            string newBody = null)
        {
            var lowerName = targetName.ToLower();

            //Ideally use .SelectSingleNode, but doesnt work with names with ':' character and is less flexible
            //var selectedNode = node.SelectSingleNode(lowerName);
            
            var selectedNodes = outerNode.Descendants().Where(child =>
            {
                return child.Name.ToLower() == lowerName;
            }).ToList();

            if (selectedNodes.Count > 0)
            {
                template??= NodeTemplate;
                newName??= targetName;
                newAttributes??= new List<String>();
                for (int i = 0; i < selectedNodes.Count; i++)
                {
                    var selectedNode = selectedNodes[i];
                
                
                    var parent = selectedNode.ParentNode;
                    var newNode = Convert2BlazorFromParts(template, newName, 
                        GetNewAttributes(selectedNode.Attributes, newAttributes), newBody ?? selectedNode.InnerHtml);
                    parent.ReplaceChild(newNode, selectedNode);
                }

                return true;
            }

            return false;
        }
    }
}
