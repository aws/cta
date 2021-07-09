using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class DirectiveConverter
    {
        private const string AttributeNameRegexGroupName = "AttributeName";
        private const string AttributeValueRegexGroupName = "AttributeValue";
        private const string DefaultAttributeMigrationTemplate = "<!-- Conversion of {0} attribute (value: {1}) for {2} directive not currently supported -->";
        private const string DefaultDirectiveMigrationTemplate = "<!-- General conversion for {0} directive not currently supported -->";

        private static Regex AttributeSplitRegex { get { return new Regex(@"(?<AttributeName>[^\s=]+)\s*=\s*(?<AttributeValue>\S+)"); } }

        private protected virtual IEnumerable<string> AttributeWhitelist { get { return Enumerable.Empty<string>(); } }

        public string ConvertDirective(string directiveName, string directiveString)
        {
            // TODO: We want to make sure that certain directives only occur once (i.e. layout,
            // inherits, etc.) Need to find a better way to detect and deal with those or perhaps
            // just leave it up the the developer given that detecting which layout or base class
            // to choose is difficult
            var migrationResultsContent = GetMigratedAttributes(directiveString, directiveName)
                // Want to ensure that the general directive conversion stays at the front if it exists
                .Prepend(GetMigratedDirective(directiveName))
                .Select(migrationResult => migrationResult.Content);

            return string.Join(Environment.NewLine, migrationResultsContent);
        }

        // We want to allow this to be overridden in case some specialized functionality is required to
        // perform the conversion, similar to the reasoning behind using Func<> in the attribute map
        private protected virtual DirectiveMigrationResult GetMigratedDirective(string directiveName)
        {
            return new DirectiveMigrationResult(DirectiveMigrationResultType.Comment, string.Format(DefaultDirectiveMigrationTemplate, directiveName));
        }

        private IEnumerable<DirectiveMigrationResult> GetMigratedAttributes(string directiveString, string directiveName)
        {
            return AttributeSplitRegex
                .Matches(directiveString)
                .SelectMany(match => {
                    var attrName = match.Groups[AttributeNameRegexGroupName].Value;
                    var attrValue = match.Groups[AttributeValueRegexGroupName].Value;

                    return AttributeWhitelist.Contains(attrName, StringComparer.InvariantCultureIgnoreCase)
                        ? UniversalDirectiveAttributeMap.AttributeMap[attrName](attrValue)
                        : new[] { new DirectiveMigrationResult(
                            DirectiveMigrationResultType.Comment,
                            string.Format(DefaultAttributeMigrationTemplate, attrName, attrValue, directiveName)) };
                })
                // Keep the order of results as Directives -> Comments -> New HTML nodes
                .OrderBy(migrationResult => (int)migrationResult.MigrationResultType)
                // Eliminate any duplicate statements
                .Distinct();
        }
    }
}
