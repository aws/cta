using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class ButtonControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>()
                {
                    ["onclick"] = "@onclick", 
                    ["cssclass"] = "class"
                };
            } 
        }
        protected override string BlazorName { get { return "button"; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            string buttonText = "";
            try
            {
                var textAttr = node.Attributes.AttributesWithName("text").Single();
                buttonText = textAttr.Value;
            }
            catch (InvalidOperationException ex)
            {
                // Todo: Throw warning about button with no text
            }
            
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, ConvertAttributes(node.Attributes), buttonText);
        }
    }
}
