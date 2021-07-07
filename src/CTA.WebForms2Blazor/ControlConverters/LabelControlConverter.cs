using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class LabelControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>()
                {
                    ["id"] = "id"
                };
            } 
        }
        protected override string BlazorName { get { return "label"; } }

        //protected override string NodeTemplate { get { return @"{2}"; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var labelText = "";
            try
            {
                var textAttr = node.Attributes.AttributesWithName("text").Single();
                labelText = textAttr.Value;
            }
            catch (InvalidOperationException ex)
            {
                //Todo: Throw warning about label with no text
            }
            
            //Not sure if this needs to be handled and if this is expected behavior
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, ConvertAttributes(node.Attributes), labelText);
        }
    }
}
