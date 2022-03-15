using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Helpers.TagConversion;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateConditions
{
    /// <summary>
    /// Represents a condition that is met if any of the conditions
    /// it contains are met.
    /// </summary>
    public class AnyConditionTemplateCondition : TemplateCondition
    {
        /// <summary>
        /// The set of conditions to check.
        /// </summary>
        public IEnumerable<TemplateCondition> Conditions { get; set; }

        /// <inheritdoc/>
        public override void Validate(bool isBaseCondition)
        {
            base.Validate(isBaseCondition);

            if (Conditions == null || !Conditions.Any())
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template condition, " +
                    $"expected Conditions to have a value but was null or empty");
            }

            foreach (var condition in Conditions ?? Enumerable.Empty<TemplateCondition>())
            {
                condition.Validate(false);
            }
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return Conditions?.Any(condition => condition.ConditionIsMet(node)) ?? false;
        }
    }
}
