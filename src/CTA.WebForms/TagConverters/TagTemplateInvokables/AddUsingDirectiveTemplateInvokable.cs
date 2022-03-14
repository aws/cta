using System;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateInvokables
{
    /// <summary>
    /// Represents an invokable procedure which ensures that a given
    /// <c>@using</c> directive is included at the top of the view file
    /// that is currently being ported.
    /// </summary>
    public class AddUsingDirectiveTemplateInvokable : ITemplateInvokable
    {
        /// <summary>
        /// The namespace to be included with the <c>@using</c> directive.
        /// </summary>
        public string NamespaceName { get; set; }

        /// <inheritdoc/>
        public bool Validate()
        {
            return !string.IsNullOrEmpty(NamespaceName);
        }

        /// <inheritdoc/>
        public void Invoke(HtmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
