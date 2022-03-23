using CTA.WebForms.Helpers.TagConversion;

namespace CTA.WebForms.TagConverters.TagTemplateInvokables
{
    /// <summary>
    /// Represents an invokable procedure which ensures that a given
    /// <c>@using</c> directive is included at the top of the view file
    /// that is currently being ported.
    /// </summary>
    public class AddUsingDirectiveTemplateInvokable : TemplateInvokable
    {
        /// <summary>
        /// The namespace to be included with the <c>@using</c> directive.
        /// </summary>
        public string NamespaceName { get; set; }

        /// <inheritdoc/>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(NamespaceName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template invokable, " +
                    $"expected NamespaceName to have a value but was null or empty");
            }
        }

        /// <inheritdoc/>
        public override void Invoke()
        {
            _viewImportService.AddViewImport($"@using {NamespaceName}");
        }
    }
}
