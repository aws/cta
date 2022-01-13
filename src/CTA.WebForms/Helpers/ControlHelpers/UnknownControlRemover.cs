using System.Text.RegularExpressions;

namespace CTA.WebForms.Helpers.ControlHelpers
{
    public class UnknownControlRemover
    {
        private const string StartTagCommentTemplate = "@* The following tag is not supported: {0} *@";

        /// <summary>
        /// Used to identify start tags that follow web forms user control format, it will match
        /// if the name consists of 2 strings of word characters separated by a single colon
        /// </summary>
        public static readonly Regex ControlStartTagRegex = new Regex(@"<\w+:\w+[^>]*>");
        /// <summary>
        /// Used to identify end tags that follow web forms user control format, it will match
        /// if the name consists of 2 strings of word characters separated by a single colon
        /// </summary>
        public static readonly Regex ControlEndTagRegex = new Regex(@"</\w+:\w+[^>]*>");

        public static string RemoveUnknownTags(string htmlString)
        {
            htmlString = ControlStartTagRegex.Replace(htmlString, match => string.Format(StartTagCommentTemplate, match.Value));
            htmlString = ControlEndTagRegex.Replace(htmlString, match => string.Format(Constants.RazorServerSideCommentTemplate, match.Value));

            return htmlString;
        }
    }
}
