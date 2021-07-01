using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class LabelControlConverter : ControlConverter
    {
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                throw new NotImplementedException();
            } 
        }
        protected override string BlazorName { get { throw new NotImplementedException(); } }
        
        public LabelControlConverter() : base()
        {
            
        }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
