using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public abstract class ControlConverter
    {
        protected ControlConverter()
        {
            //Constructor might not be needed
        }
        
        //Might be better to make methods static?
        public virtual HtmlNode Convert2Blazor(HtmlNode node)
        {
            return node;
        }
    }
}
