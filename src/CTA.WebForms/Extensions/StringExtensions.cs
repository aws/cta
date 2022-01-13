namespace CTA.WebForms.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveOuterQuotes(this string input)
        {
            if (input.Length > 1 && ((input.StartsWith("\"") && input.EndsWith("\"")) || (input.StartsWith("'") && input.EndsWith("'"))))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }

        public static string ConvertToRazorComment(this string input)
        {
            var commentContent = input ?? string.Empty;
            return string.Format(Constants.MarkupCommentTemplate, commentContent);
        }
    }
}
