using System.Text.RegularExpressions;

namespace CTA.Rules.Common.Extensions
{
    public static class StringExtensions
    {
        public static bool WildcardEquals(this string source, string compareString)
        {
            var regexSource = new Regex(
                Regex.Escape(source).Replace(@"\*", ".*").Replace(@"\?", "."),
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var regexCompareString = new Regex(
                Regex.Escape(compareString).Replace(@"\*", ".*").Replace(@"\?", "."),
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return regexSource.IsMatch(compareString) || regexCompareString.IsMatch(source);
        }
    }
}
