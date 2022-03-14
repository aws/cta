using System;

namespace CTA.WebForms.Services
{
    /// <summary>
    /// Service class used to link code behind files to their corresponding
    /// view files. This makes simultaneous conversion of tags in the view
    /// layer and code behind references possible.
    /// </summary>
    public class CodeBehindReferenceLinkerService
    {
        /// <summary>
        /// Interacts with appropriate code behind converter to replace references
        /// to the given tag as specified by the provided code behind handler and
        /// retrieves a binding to the generated property if it exists.
        /// </summary>
        /// <returns>Property binding text if a bindable property was generated, null otherwise.</returns>
        public string GetBindingValueIfExists()
        {
            throw new NotImplementedException();
        }
    }
}
