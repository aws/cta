using System;
using System.Collections.Generic;
using System.Linq;

namespace CTA.WebForms.DirectiveConverters
{
    public class ControlDirectiveConverter : DirectiveConverter
    {
        private Dictionary<string, IEnumerable<DirectiveMigrationResult>> OverrideMap
        {
            get
            {
                return new Dictionary<string, IEnumerable<DirectiveMigrationResult>>()
                {
                    [UniversalDirectiveAttributeMap.InheritsAttr] = Enumerable.Empty<DirectiveMigrationResult>(),
                };
            }
        }
        private protected override IEnumerable<string> AttributeAllowList
        {
            get
            {
                return new[]
                {
                    UniversalDirectiveAttributeMap.CodeBehind,
                    UniversalDirectiveAttributeMap.LanguageAttr,
                    UniversalDirectiveAttributeMap.ClassNameAttr,
                    UniversalDirectiveAttributeMap.SourceAttr,
                };
            }
        }
        
        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedDirectives(string directiveName, string originalFilePath)
        {
            return Enumerable.Empty<DirectiveMigrationResult>();
        }
        
        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedAttributes(string directiveString, string directiveName, string projectName)
        {
            return AttributeSplitRegex
                .Matches(directiveString)
                .SelectMany(match => {
                    var attrName = match.Groups[AttributeNameRegexGroupName].Value;
                    var attrValue = match.Groups[AttributeValueRegexGroupName].Value;
                    if (OverrideMap.ContainsKey(attrName))
                    {
                        return OverrideMap[attrName];
                    }
                    if (AttributeAllowList.Contains(attrName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return UniversalDirectiveAttributeMap.AttributeMap[attrName](new[] {attrValue});
                    }
                    return new[] { new DirectiveMigrationResult(
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
