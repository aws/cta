using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateConditions
{
    /// <summary>
    /// Represents a condition that is met if all of the conditions
    /// it contains are met.
    /// </summary>
    public class AllConditionsTemplateCondition : TemplateCondition
    {
        /// <summary>
        /// The set of conditions to check.
        /// </summary>
        public IEnumerable<TemplateCondition> Conditions { get; set; }

        /// <inheritdoc/>
        public override bool ConditionIsMet(HtmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
