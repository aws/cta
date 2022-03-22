using CTA.WebForms.TagCodeBehindHandlers;
using CTA.WebForms.TagConverters;
using HtmlAgilityPack;

namespace CTA.WebForms.Helpers.TagConversion
{
    public class TagConversionAction
    {
        public HtmlNode Node { get; }
        public TagConverter Converter { get; }
        public ITagCodeBehindHandler CodeBehindHandler { get; }

        public TagConversionAction(HtmlNode node, TagConverter converter)
        {
            Node = node;
            Converter = converter;
            CodeBehindHandler = converter.GetCodeBehindHandlerInstance();
        }
    }
}
