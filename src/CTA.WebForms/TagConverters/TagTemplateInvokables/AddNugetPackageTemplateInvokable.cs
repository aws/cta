using CTA.WebForms.Helpers.TagConversion;

namespace CTA.WebForms.TagConverters.TagTemplateInvokables
{
    /// <summary>
    /// Represents an invokable procedure which ensures that a given
    /// nuget package is included in the WebForms project that is currently
    /// being ported.
    /// </summary>
    public class AddNugetPackageTemplateInvokable : TemplateInvokable
    {
        /// <summary>
        /// The name of the nuget package to be included.
        /// </summary>
        public string PackageName { get; set; }

        /// <inheritdoc/>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(PackageName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template invokable, " +
                    $"expected PackageName to have a value but was null or empty");
            }
        }

        /// <inheritdoc/>
        public override void Invoke()
        {
            _viewImportService.AddNuGetPackage(PackageName);
        }
    }
}
