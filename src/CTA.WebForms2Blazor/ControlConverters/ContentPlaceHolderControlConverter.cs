using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class ContentPlaceHolderControlConverter : ControlConverter
    {
        protected override string NodeTemplate
        {
            get { return @"{2}"; }
        }

        protected override string BlazorName { get { return ""; } }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, null, "@Body");
        }
    }
}
