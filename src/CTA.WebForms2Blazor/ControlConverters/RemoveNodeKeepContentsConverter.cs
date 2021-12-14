using System.Collections.Generic;
using System.Net;
using CommandLine;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class RemoveNodeKeepContentsConverter : ControlConverter
    {
        protected override string BlazorName { get { return ""; } }

        //This function removes the specified node but keeps the inner contents
        //Returns null
        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            DeleteNode(node, true);
            return null;
        }
    }
}
