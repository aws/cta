using CTA.WebForms.Services;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters
{
    /// <summary>
    /// An abstract converter capable of porting view layer tags of a given
    /// type to an alternate representation. This includes porting related
    /// to code behind references.
    /// </summary>
    public abstract class TagConverter
    {
        private protected CodeBehindReferenceLinkerService _codeBehindLinkerService;

        /// <summary>
        /// Used to inject any necessary services and perform other initialization
        /// steps required before tag migration.
        /// </summary>
        /// <param name="codeBehindLinkerService">The service instance that will
        /// be used to convert tag code behind references.</param>
        public virtual void Initialize(CodeBehindReferenceLinkerService codeBehindLinkerService)
        {
            _codeBehindLinkerService = codeBehindLinkerService;
        }

        /// <summary>
        /// Replaces the provided <see cref="HtmlNode"/> with its Blazor equivalent
        /// as specified by the given converter.
        /// </summary>
        /// <param name="node">The tag to be replaced.</param>
        public abstract void MigrateTag(HtmlNode node);

        /// <summary>
        /// Checks whether the properties of this converter form a valid configuration.
        /// </summary>
        /// <returns><c>true</c> if the converter has been initialized to a valid configuration,
        /// <c>false</c> otherwise.</returns>
        public abstract bool Validate();
    }
}
