using System;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateConditions
{
    /// <summary>
    /// Represents a condition that is met if the <see cref="HtmlNode"/> to be
    /// converted has a specific attribute with any value.
    /// </summary>
    public class HasAttributeTemplateCondition : TemplateCondition
    {
        /// <summary>
        /// The attribute name to check for in the the given <see cref="HtmlNode"/>'s
        /// attribute set.
        /// </summary>
        public string AttributeName { get; set; }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
