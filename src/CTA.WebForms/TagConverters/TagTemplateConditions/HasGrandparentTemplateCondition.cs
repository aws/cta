using CTA.WebForms.Helpers.TagConversion;
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
        public override void Validate(bool isBaseCondition)
        {
            base.Validate(isBaseCondition);

            if (string.IsNullOrEmpty(GrandparentTagName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template condition, " +
                    $"expected GrandparentTagName to have a value but was null or empty");
            }
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return node.ParentNode?.ParentNode?.Name.Equals(GrandparentTagName) ?? false;
        }
    }
}
