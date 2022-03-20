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
        private protected ViewImportService _viewImportService;

        /// <summary>
        /// The name of the tag to apply this converter to.
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// The type representation of the source tag in code behind
        /// files, if such a type exists.
        /// </summary>
        public string CodeBehindType { get; set; }
        /// <summary>
        /// The name of the type to be used for conversion of code behind
        /// references to the source tag. In most cases <see cref="DefaultTagCodeBehindHandler"/>
        /// will be sufficient.
        /// </summary>
        public string CodeBehindHandler { get; set; }
        /// <summary>
        /// The set of conditions used to decide which template to use on any
        /// given source tag.
        /// </summary>

        /// <summary>
        /// Used to inject any necessary services and perform other initialization
        /// steps required before tag migration.
        /// </summary>
        /// <param name="codeBehindLinkerService">The service instance that will
        /// be used to convert tag code behind references.</param>
        /// <param name="viewImportService">The service instance that will be used
        /// for adding any view imports needed for the conversion.</param>
        public virtual void Initialize(
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            ViewImportService viewImportService)
        {
            _codeBehindLinkerService = codeBehindLinkerService;
            _viewImportService = viewImportService;
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
        public abstract void Validate();
    }
}
