using CTA.WebForms.Helpers.TagConversion;
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
        public override void Validate(bool isBaseCondition)
        {
            base.Validate(isBaseCondition);

            if (string.IsNullOrEmpty(ParentTagName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template condition, " +
                    $"expected ParentTagName to have a value but was null or empty");
            }
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return node.ParentNode?.Name.Equals(ParentTagName) ?? false;
        }
    }
}
