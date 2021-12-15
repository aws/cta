using HtmlAgilityPack;

namespace CTA.WebForms.ControlConverters
{
    public class RemoveNodeAndContentsConverter : ControlConverter
    {
        protected override string BlazorName { get { return ""; } }

        //This function removes the specified node and the inner contents
        //Returns null
        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            DeleteNode(node, false);
            return null;
        }
    }
}
