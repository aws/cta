using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Services;

namespace CTA.WebForms.TagConverters.TagTemplateInvokables
{
    /// <summary>
    /// Represents an abstract invokable procedure that brings extra functionality
    /// outside of immediate view and code behind conversions to sub types of
    /// <see cref="TagConverter"/>.
    /// </summary>
    public abstract class TemplateInvokable
    {
        private protected ViewImportService _viewImportService;

        /// <summary>
        /// The templates that this invokable applies to. If null or empty,
        /// the invokable applies to all templates in the parent converter.
        /// </summary>
        public IEnumerable<string> ForTemplates { get; set; }

        /// <summary>
        /// Checks whether the current invokable should be used for the chosen template.
        /// </summary>
        /// <param name="templateName">The template that is currently being evaluated.</param>
        /// <returns><c>true</c> if the condition should be checked, <c>false</c> otherwise.</returns>
        public bool ShouldInvoke(string templateName)
        {
            return ForTemplates == null
                || ForTemplates.Count() == 0
                || ForTemplates.Contains(templateName);
        }

        /// <summary>
        /// Used to inject any necessary services and perform other initialization
        /// steps required before invocation.
        /// </summary>
        /// <param name="viewImportService">The service instance that will be used
        /// for adding any view imports needed for the conversion.</param>
        public void Initialize(ViewImportService viewImportService)
        {
            _viewImportService = viewImportService;
        }

        /// <summary>
        /// Executes this invokable procedure.
        /// </summary>
        public abstract void Invoke();

        /// <summary>
        /// Checks whether the properties of this invokable form a valid configuration.
        /// </summary>
        public abstract void Validate();
    }
}
