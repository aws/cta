using System;
using System.Text.RegularExpressions;

namespace CTA.WebForms.Helpers
{
    public class Utilities
    {
        public static Regex InvalidNamespaceIdentifierCharactersRegex => new Regex(@"[^\w.]");
        public static Regex DoublePeriodRegex => new Regex(@"[.]{2,}");
        public static Regex ValidNamespaceIdentifierStart => new Regex(@"^[a-zA-Z_]");
        public static Regex UnderscoreReplaceableCharacters => new Regex(@"[- ]");

        public static string SeparateStringsWithNewLine(params string[] strings)
        {
            return strings == null
                ? string.Empty
                : string.Join(Environment.NewLine, strings);
        }

        public static string NormalizeNamespaceIdentifier(string namespaceIdentifier)
        {
            // NOTE: When creating a project with spaces or hyphens in it, each space/hyphen
            // turns into an underscore, they don't get compressed into one
            namespaceIdentifier = UnderscoreReplaceableCharacters.Replace(namespaceIdentifier, "_");
            namespaceIdentifier = InvalidNamespaceIdentifierCharactersRegex.Replace(namespaceIdentifier, string.Empty);
            namespaceIdentifier = DoublePeriodRegex.Replace(namespaceIdentifier, ".");

            var isValidStart = ValidNamespaceIdentifierStart.IsMatch(namespaceIdentifier);
            if (!isValidStart)
            {
                namespaceIdentifier = "_" + namespaceIdentifier;
            }

            return namespaceIdentifier;
        }
    }
}
