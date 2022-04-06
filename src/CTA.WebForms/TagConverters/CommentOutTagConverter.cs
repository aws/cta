using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.TagCodeBehindHandlers;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters
{
    /// <summary>
    /// A converter that will use a server side comment to
    /// remove a given node and its contents.
    /// </summary>
    public class CommentOutTagConverter : TagConverter
    {
        /// <inheritdoc/>
        public override Task MigrateTagAsync(HtmlNode node, string viewFilePath, TagCodeBehindHandler handler, int taskId)
        {
            var doc = node.OwnerDocument;
            var parent = node.ParentNode;

            node.InnerHtml = node.InnerHtml
                .Replace("@*", "")
                .Replace("*@", "");

            var before = doc.CreateTextNode("@*");
            var after = doc.CreateTextNode("*@");

            parent.InsertBefore(before, node);
            parent.InsertAfter(after, node);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(TagName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate comment out tag converter, " +
                    $"expected TagName to have a value but was null or empty");
            }
        }
    }
}
