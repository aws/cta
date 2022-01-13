using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;

namespace CTA.WebForms.Helpers.ControlHelpers
{
    public class ControlConversionAction
    {
        public HtmlNode Node { get; }
        public HtmlNode Parent { get; }
        public ControlConverter ControlConverter { get; }
        public ControlConversionAction(HtmlNode node, HtmlNode parent, ControlConverter controlConverter)
        {
            Node = node;
            Parent = parent;
            ControlConverter = controlConverter;
        }
    }
}
