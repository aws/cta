using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using HtmlAgilityPack;
using Microsoft.DotNet.PlatformAbstractions;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public abstract class ControlConverter
    {
        protected virtual Dictionary<String, String> AttributeMap
        {
            get { return new Dictionary<string, string>(); }
        }

        protected virtual IEnumerable<ViewLayerControlAttribute> NewAttributes
        {
            get { return new List<ViewLayerControlAttribute>(); }
        }
        protected abstract string BlazorName { get; }
        protected virtual string NodeTemplate { get { return @"<{0} {1}>{2}</{0}>"; } }

        protected ControlConverter()
        {
            //Constructor might not be needed
        }
        
        //Passing this method through every .CreateNode ensures that all nodes have original capitalization
        public static void PreserveCapitalization(HtmlDocument htmlDocument)
        {
            htmlDocument.OptionOutputOriginalCase = true;
        }
        
        public virtual HtmlNode Convert2Blazor(HtmlNode node)
        {
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, JoinAllAttributes(node.Attributes, NewAttributes), node.InnerHtml);
        }
        
        protected HtmlNode Convert2BlazorFromParts(string template, string name, string attributes, string body)
        {
            string newContent = String.Format(template, name, attributes, body);
            HtmlNode newNode = HtmlNode.CreateNode(newContent, PreserveCapitalization);
            
            return newNode;
        }

        protected virtual string JoinAllAttributes(HtmlAttributeCollection oldAttributes,
            IEnumerable<ViewLayerControlAttribute> additionalAttributes)
        {
            var convertedAttributes = ConvertAttributes(oldAttributes);
            
            //This Union makes sures that if any attribute with the same name is added, only the original one is kept
            var combinedAttributes = additionalAttributes == null ? convertedAttributes : convertedAttributes.Union(additionalAttributes);
            
            var attributeStringList = combinedAttributes.Select(attr => attr.ToString());
            var combinedAttributesString = string.Join(" ", attributeStringList);
            return combinedAttributesString;
        }

        protected IEnumerable<ViewLayerControlAttribute> ConvertAttributes(HtmlAttributeCollection attributeCollection)
        {
            var convertedAttributes = attributeCollection
                .Where(attr => AttributeMap.ContainsKey(attr.Name))
                .Select(attr =>
                {
                    if (attr.QuoteType == AttributeValueQuote.DoubleQuote)
                    {
                        return new ViewLayerControlAttribute($"{AttributeMap[attr.Name]}", $"\"{attr.Value}\"");
                    }
                    return new ViewLayerControlAttribute($"{AttributeMap[attr.Name]}", $"'{attr.Value}'");
                });
            
            return convertedAttributes;
        }

        public static string ConvertEmbeddedCode(string htmlString)
        {
            MatchEvaluator dataBindEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceDataBind);
            MatchEvaluator singleExprEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceSingleExpr);
            MatchEvaluator directiveEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceDirective);
            MatchEvaluator aspExprEval = new MatchEvaluator(EmbeddedCodeReplacers.ReplaceAspExpr);
            
            htmlString = EmbeddedCodeReplacers.DataBindRegex.Replace(htmlString, dataBindEval);
            htmlString = EmbeddedCodeReplacers.SingleExpRegex.Replace(htmlString, singleExprEval);
            htmlString = EmbeddedCodeReplacers.DirectiveRegex.Replace(htmlString, directiveEval);

            //Not implemented/used yet
            //htmlString = EmbeddedCodeReplacers.AspExpRegex.Replace(htmlString, aspExprEval);
            
            return htmlString;
        }
        
        //This function only updates the all child nodes that matches the name
        public bool UpdateInnerHtmlNode(HtmlNode outerNode, string targetName, string id = null,
            string template = null, 
            string newName = null, 
            IEnumerable<ViewLayerControlAttribute> addedAttributes = null, 
            string newBody = null)
        {
            var selectedNodes = outerNode.Descendants().Where(child =>
            {
                return String.Equals(child.Name, targetName, StringComparison.InvariantCultureIgnoreCase);
            }).ToList();
            if (!string.IsNullOrEmpty(id))
            {
                selectedNodes = outerNode.Descendants().Where(child =>
                {
                    return String.Equals(child.Name, targetName, StringComparison.InvariantCultureIgnoreCase) &&
                           String.Equals(child.Id, id, StringComparison.InvariantCultureIgnoreCase);
                }).ToList();
            }
            
            if (selectedNodes.Count == 0)
            {
                return false;
            }
            
            template ??= NodeTemplate;
            newName ??= targetName;
            for (int i = 0; i < selectedNodes.Count; i++)
            {
                var selectedNode = selectedNodes[i];
                var parent = selectedNode.ParentNode;
                    
                var joinedAttributesString = JoinAllAttributes(selectedNode.Attributes, addedAttributes);
                var bodyContent = newBody ?? selectedNode.InnerHtml;
                var newNode = Convert2BlazorFromParts(template, newName, joinedAttributesString, bodyContent);
                    
                parent.ReplaceChild(newNode, selectedNode);
            }

            return true;
        }

        public void DeleteNode(HtmlNode node, bool keepContents)
        {
            var parent = node.ParentNode;
            if (keepContents)
            {
                var childNodes = node.ChildNodes;
                parent.AppendChildren(childNodes);   
            }
            
            parent.RemoveChild(node);
        }
    }
}
