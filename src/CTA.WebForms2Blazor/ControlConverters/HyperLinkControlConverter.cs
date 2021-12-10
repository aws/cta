using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class HyperLinkControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get 
            { 
                return new Dictionary<string, string>()
                {
                    ["navigateurl"] = "href", 
                    ["cssclass"] = "class"
                };
                
            } 
        }
        protected override string BlazorName { get { return "a"; } }

    }
}
