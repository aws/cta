using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public abstract class ControlConverter
    {
        protected abstract Dictionary<String, String> AttributeMap { get; }
        protected abstract string BlazorName { get; }
        protected virtual string NodeTemplate { get { return @"<{0} {1}>{2}</{0}>"; } }
        
        //DataBind regex might have issue of taking front parentheses
        protected static Regex DataBindRegex = new Regex("<%#\\W*(?<expr>[^%>]*)\\s*%>");
        protected static Regex SingleExpRegex = new Regex("<%:\\s*(?<expr>[^%>]*)\\s*%>");
        protected static Regex DirectiveRegex = new Regex("<%@\\s*(?<expr>[^%>]*)\\s*%>");
        protected static Regex AspExpRegex = new Regex("<%\\$\\s*(?<expr>[^%>]*)\\s*%>");
        
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
                .Select(attr => $"{AttributeMap[attr.Name]}='{attr.Value}'");

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
            
            var dataBindMatcher = DataBindRegex.Matches(htmlString);
            var singleExprMatcher = SingleExpRegex.Matches(htmlString);
            var directiveMatcher = DirectiveRegex.Matches(htmlString);
            var aspExprMatcher = AspExpRegex.Matches(htmlString);
            
            //Might be a better way to do this
            foreach (Match matchObj in dataBindMatcher)
            {
                if (matchObj.Success)
                {
                    var expr = matchObj.Groups["expr"].Value;
                    var newValue = "@(" + expr + ")";
                    htmlString = htmlString.Replace(matchObj.Value, newValue);
                }
            }
            
            foreach (Match matchObj in singleExprMatcher)
            {
                if (matchObj.Success)
                {
                    var expr = matchObj.Groups["expr"].Value;
                    var newValue = "@" + expr;
                    htmlString = htmlString.Replace(matchObj.Value, newValue);
                }
            }
            
            foreach (Match matchObj in directiveMatcher)
            {
                if (matchObj.Success)
                {
                    var expr = matchObj.Groups["expr"].Value;
                    // Not handled yet, replace newValue with actual replacement
                    // var newValue = "@(" + expr + ")";
                    // htmlString = htmlString.Replace(matchObj.Value, newValue);
                }
            }
            
            foreach (Match matchObj in aspExprMatcher)
            {
                if (matchObj.Success)
                {
                    var expr = matchObj.Groups["expr"].Value;
                    // Not handled yet, replace newValue with actual replacement
                    // var newValue = "@(" + expr + ")";
                    // htmlString = htmlString.Replace(matchObj.Value, newValue);
                }
            }

            var res = new HtmlDocument();
            res.LoadHtml(htmlString);
            return res;
        }
    }
}
