using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CTA.WebForms.Extensions
{
    public static class RegexExtensions
    {
        /// <summary>
        /// Allows for regex replacements using async function delegates.
        /// </summary>
        /// <param name="regex">The regular expression that is being used.</param>
        /// <param name="input">The string that <paramref name="regex"/> is being applied to.</param>
        /// <param name="replacementFn">The function that is used for match replacement.</param>
        /// <returns>The string <paramref name="input"/> with all matches made by <paramref name="regex"/>
        /// replaced as specified in <paramref name="replacementFn"/>.</returns>
        public static async Task<string> ReplaceAsync(this Regex regex, string input, Func<Match, Task<string>> replacementFn)
        {
            var sb = new StringBuilder();
            var lastIndex = 0;

            foreach (Match match in regex.Matches(input))
            {
                sb.Append(input, lastIndex, match.Index - lastIndex)
                  .Append(await replacementFn(match).ConfigureAwait(false));

                lastIndex = match.Index + match.Length;
            }

            sb.Append(input, lastIndex, input.Length - lastIndex);
            return sb.ToString();
        }
    }
}
