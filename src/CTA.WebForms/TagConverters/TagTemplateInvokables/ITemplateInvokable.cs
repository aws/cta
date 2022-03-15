using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters.TagTemplateInvokables
{
    /// <summary>
    /// Represents an abstract invokable procedure that brings extra functionality
    /// outside of immediate view and code behind conversions to sub types of
    /// <see cref="TagConverter"/>.
    /// </summary>
    public interface ITemplateInvokable
    {
        /// <summary>
        /// Executes this invokable procedure.
        /// </summary>
        /// <param name="node">The <see cref="HtmlNode"/> whose conversion triggered
        /// this invokable procedure.</param>
        public void Invoke(HtmlNode node);

        /// <summary>
        /// Checks whether the properties of this invokable form a valid configuration.
        /// </summary>
        public void Validate();
    }
}
