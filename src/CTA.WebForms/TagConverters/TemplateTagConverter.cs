using System.Collections.Generic;
using CTA.WebForms.TagCodeBehindHandlers;
using CTA.WebForms.TagConverters.TagTemplateConditions;
using CTA.WebForms.TagConverters.TagTemplateInvokables;

namespace CTA.WebForms.TagConverters
{
    /// <summary>
    /// A converter that utilizes a series of templates denoting
    /// optional alternate configurations to port view layer tags
    /// of a given type.
    /// </summary>
    public class TemplateTagConverter : ITagConverter
    {
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
        public IEnumerable<TemplateCondition> Conditions { get; set; }
        /// <summary>
        /// The set of actions to be taken outside of normal view or code
        /// behind conversion.
        /// </summary>
        public IEnumerable<ITemplateInvokable> Invocations { get; set; }
        /// <summary>
        /// The set of temlate options to be used for source tags of the
        /// given type.
        /// </summary>
        public IDictionary<string, string> Templates { get; set; }
    }
}
