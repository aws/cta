using System;
using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Helpers.TagConversion;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateConditions
{
    /// <summary>
    /// Represents an abstract condition that must be met for one or more
    /// templates to be usable as a replacement for a given tag.
    /// </summary>
    public abstract class TemplateCondition
    {
        /// <summary>
        /// The templates that this condition applies to. If null or empty,
        /// the condition applies to all templates in the parent converter.
        /// </summary>
        public IEnumerable<string> ForTemplates { get; set; }

        /// <summary>
        /// Checks whether the current condition should be checked when evaluating if a given template
        /// should be used.
        /// </summary>
        /// <param name="templateName">The template that is currently being evaluated.</param>
        /// <returns><c>true</c> if the condition should be checked, <c>false</c> otherwise.</returns>
        public bool ShouldCheckCondition(string templateName)
        {
            return ForTemplates == null
                || ForTemplates.Count() == 0
                || ForTemplates.Contains(templateName);
        }

        /// <summary>
        /// Checks whether the properties of this condition form a valid configuration.
        /// </summary>
        /// <param name="isBaseCondition">Whether or not this condition is at the base level of
        /// the converter's condition set (i.e. not a sub condition of <see cref="AnyConditionTemplateCondition"/>
        /// or <see cref="AllConditionsTemplateCondition"/>.</param>
        public virtual void Validate(bool isBaseCondition)
        {
            // We only want base conditions to specify templates that they
            // apply to for simplicity's sake
            if (!isBaseCondition && ForTemplates != null)
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template condition, " +
                    $"ForTemplates has been set despite not being a base condition");
            }
        }

        /// <summary>
        /// Checks whether the condition is met by <paramref name="node"/> and returns the result.
        /// </summary>
        /// <param name="node">The <see cref="HtmlNode"/> that the converter is running on.</param>
        /// <returns><c>true</c> if condition is met, <c>false</c> otherwise.</returns>
        public abstract bool ConditionIsMet(HtmlNode node);
    }
}
