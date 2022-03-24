using System;
using System.Linq;
using CTA.WebForms.TagCodeBehindHandlers;
using CTA.WebForms.TagConverters;
using HtmlAgilityPack;

namespace CTA.WebForms.Helpers.TagConversion
{
    public class TagConversionAction
    {
        public HtmlNode Node { get; }
        public TagConverter Converter { get; }
        public TagCodeBehindHandler CodeBehindHandler { get; }

        public TagConversionAction(HtmlNode node, TagConverter converter)
        {
            Node = node;
            Converter = converter;

            var idValue = node.Attributes
                .Where(attr => attr.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault()?.Value;

            CodeBehindHandler = converter.GetCodeBehindHandlerInstance(idValue);
        }
    }
}
