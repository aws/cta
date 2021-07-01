using CTA.WebForms2Blazor.ControlConverters;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public class ControlConversionAction
    {
        public HtmlNode Node { get; }
        public HtmlNode Parent { get; }
        public ControlConverter Rules { get; }
        public ControlConversionAction(HtmlNode node, HtmlNode parent, ControlConverter rules)
        {
            Node = node;
            Parent = parent;
            Rules = rules;
        }
    }
}
