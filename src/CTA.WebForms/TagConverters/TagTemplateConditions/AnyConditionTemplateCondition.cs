using System.Collections.Generic;
using System.Linq;
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
        public override bool Validate(bool isBaseCondition)
        {
            return base.Validate(isBaseCondition)
                && Conditions != null
                && Conditions.Any()
                && Conditions.All(condition => condition.Validate(false));
        }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            return Conditions?.Any(condition => condition.ConditionIsMet(node)) ?? false;
        }
    }
}
