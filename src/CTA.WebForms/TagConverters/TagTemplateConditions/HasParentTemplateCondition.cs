using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateConditions
{
    /// <summary>
    /// Represents a condition that is met if the <see cref="HtmlNode"/> to be
    /// converted has a specific parent tag.
    /// </summary>
    public class HasParentTemplateCondition : TemplateCondition
    {
        /// <summary>
        /// The tag name to compare the given <see cref="HtmlNode"/>'s parent
        /// against.
        /// </summary>
        public string ParentTagName { get; set; }

        /// <inheritdoc/>
        public override bool Validate(bool isBaseCondition)
        {
            return base.Validate(isBaseCondition) && !string.IsNullOrEmpty(ParentTagName);
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return node.ParentNode?.Name.Equals(ParentTagName) ?? false;
        }
    }
}
