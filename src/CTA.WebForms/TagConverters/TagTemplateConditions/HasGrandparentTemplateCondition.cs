using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateConditions
{
    /// <summary>
    /// Represents a condition that is met if the <see cref="HtmlNode"/> to be
    /// converted has a specific grandparent tag.
    /// </summary>
    public class HasGrandparentTemplateCondition : TemplateCondition
    {
        /// <summary>
        /// The tag name to compare the given <see cref="HtmlNode"/>'s grandparent
        /// against.
        /// </summary>
        public string GrandparentTagName { get; set; }

        /// <inheritdoc/>
        public override bool Validate(bool isBaseCondition)
        {
            return base.Validate(isBaseCondition) && !string.IsNullOrEmpty(GrandparentTagName);
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return node.ParentNode?.ParentNode?.Name.Equals(GrandparentTagName) ?? false;
        }
    }
}
