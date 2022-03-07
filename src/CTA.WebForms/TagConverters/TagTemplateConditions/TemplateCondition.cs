using System.Collections.Generic;
using System.Linq;
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
        /// Checks whether the condition is met by <paramref name="node"/> and returns the result.
        /// </summary>
        /// <param name="node">The <see cref="HtmlNode"/> that the converter is running on.</param>
        /// <returns><c>true</c> if condition is met, <c>false</c> otherwise.</returns>
        public abstract bool ConditionIsMet(HtmlNode node);

        public bool ShouldCheckCondition(string templateName)
        {
            return ForTemplates == null
                || ForTemplates.Count() == 0
                || ForTemplates.Contains(templateName);
        }
    }
}
