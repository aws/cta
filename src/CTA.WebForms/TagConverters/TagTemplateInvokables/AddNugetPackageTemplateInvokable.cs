using System;
using CTA.WebForms.Helpers.TagConversion;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateInvokables
{
    /// <summary>
    /// Represents an invokable procedure which ensures that a given
    /// nuget package is included in the WebForms project that is currently
    /// being ported.
    /// </summary>
    public class AddNugetPackageTemplateInvokable : ITemplateInvokable
    {
        /// <summary>
        /// The name of the nuget package to be included.
        /// </summary>
        public string PackageName { get; set; }

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(PackageName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template invokable, " +
                    $"expected PackageName to have a value but was null or empty");
            }
        }

        /// <inheritdoc/>
        public void Invoke(HtmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
