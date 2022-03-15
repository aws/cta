using CTA.WebForms.Helpers.TagConversion;
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
        public override void Validate(bool isBaseCondition)
        {
            base.Validate(isBaseCondition);

            if (string.IsNullOrEmpty(AttributeName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template condition, " +
                    $"expected AttributeName to have a value but was null or empty");
            }
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return AttributeName switch
            {
                "InnerHtml" => node.InnerHtml != null && node.InnerHtml.Trim().Length > 0,
                _ => node.Attributes.Contains(AttributeName)
            };
        }
    }
}
