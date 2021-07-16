using System.Collections.Generic;
using System.Net;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class ContentControlConverter : ControlConverter
    {
        protected override string BlazorName { get { return "div"; } }

        //This function replaces the asp:Content node with an empty <div> node since removing a node entails replacing the
        //current node with its multiple child nodes, and it is currently not possible to convert a single node
        //into multiple nodes
        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, null, node.InnerHtml);
        }
    }
}
