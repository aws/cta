using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class DirectiveConverter
    {
        protected const string AttributeNameRegexGroupName = "AttributeName";
        protected const string AttributeValueRegexGroupName = "AttributeValue";
        protected const string AttributeMigrationNotSupportedTemplate = "<!-- Conversion of {0} attribute (value: {1}) for {2} directive not currently supported -->";
        protected const string DirectiveMigrationNotSupportedTemplate = "<!-- General conversion for {0} directive not currently supported -->";

        /// <summary>
        /// Each match of this regular expression represents an attribute of the Web Forms directive it is being
        /// applied to, the first named capture group, AttributeName, is the name of the attribute while the second,
        /// AttributeValue, is the value
        /// </summary>
        protected static Regex AttributeSplitRegex { get { return new Regex(@"(?<AttributeName>[^\s=]+)\s*=\s*(?<AttributeValue>[""'][^""']*[""'])"); } }

        private protected virtual IEnumerable<string> AttributeAllowList { get { return Enumerable.Empty<string>(); } }

        public string ConvertDirective(string directiveName, string directiveString, string originalFilePath, string projectName, ViewImportService viewImportService)
        {
            // TODO: We want to make sure that certain directives only occur once (i.e. layout,
            // inherits, etc.) Need to find a better way to detect and deal with those or perhaps
            // just leave it up the the developer given that detecting which layout or base class
            // to choose is difficult
            var migrationResults = GetMigratedAttributes(directiveString, directiveName, projectName);

            // Want to ensure that the general directive conversion stays at the front if it exists
            migrationResults = GetMigratedDirectives(directiveName, originalFilePath).Concat(migrationResults);

            // Remove any using directive results and send them to the viewImports service
            migrationResults = migrationResults.Where(migrationResult => {
                if (migrationResult.MigrationResultType == DirectiveMigrationResultType.UsingDirective)
                {
                    viewImportService.AddViewImport(migrationResult.Content);
                    return false;
                }

                return true;
            });

            return string.Join(Environment.NewLine, migrationResults.Select(migrationResult => migrationResult.Content));
        }

        // We want to allow this to be overridden in case some specialized functionality is required to
        // perform the conversion, similar to the reasoning behind using Func<> in the attribute map
        private protected virtual IEnumerable<DirectiveMigrationResult> GetMigratedDirectives(string directiveName, string originalFilePath)
        {
            return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.Comment, string.Format(DirectiveMigrationNotSupportedTemplate, directiveName)) };
        }

        private protected virtual IEnumerable<DirectiveMigrationResult> GetMigratedAttributes(string directiveString, string directiveName, string projectName)
        {
            return AttributeSplitRegex
                .Matches(directiveString)
                .SelectMany(match => {
                    var attrName = match.Groups[AttributeNameRegexGroupName].Value;
                    var attrValue = match.Groups[AttributeValueRegexGroupName].Value;

                    return AttributeAllowList.Contains(attrName, StringComparer.InvariantCultureIgnoreCase)
                        ? UniversalDirectiveAttributeMap.AttributeMap[attrName](attrValue)
                        : new[] { new DirectiveMigrationResult(
                            DirectiveMigrationResultType.Comment,
                            string.Format(AttributeMigrationNotSupportedTemplate, attrName, attrValue, directiveName)) };
                })
                // Keep the order of results as Directives -> Comments -> New HTML nodes
                .OrderBy(migrationResult => (int)migrationResult.MigrationResultType)
                // Eliminate any duplicate statements
                .Distinct();
        }
    }
}
