using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.Extensions;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class RegisterDirectiveConverter : DirectiveConverter
    {
        private readonly RegisteredUserControls _registeredUserControls;
        private const string IncorrectRegisterDirectiveWarning = "<!-- Register directive missing TagName or TagPrefix -->";
        private const string ControlTagName = "TagName";
        private const string ControlTagPrefix = "TagPrefix";
        private const string ControlSourceFile = "Src";
        
        private protected override IEnumerable<string> AttributeAllowList
        {
            get
            {
                return new[]
                {
                    ControlSourceFile,
                };
            }
        }
        public RegisterDirectiveConverter(RegisteredUserControls registeredUserControls)
        {
            _registeredUserControls = registeredUserControls;
        }
        
        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedDirectives(string directiveName, string originalFilePath)
        {
            return Enumerable.Empty<DirectiveMigrationResult>();
        }

        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedAttributes(string directiveString, string directiveName, string projectName)
        {
            Dictionary<string, string> attrMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            List<DirectiveMigrationResult> migratedDirectives = new List<DirectiveMigrationResult>();
            
            var attrMatches = AttributeSplitRegex.Matches(directiveString);
            foreach (Match match in attrMatches)
            {
                var attrName = match.Groups[AttributeNameRegexGroupName].Value;
                var attrValue = match.Groups[AttributeValueRegexGroupName].Value;
                attrMap.Add(attrName, attrValue.RemoveOuterQuotes());
            }

            if (attrMap.ContainsKey(ControlTagName) && attrMap.ContainsKey(ControlTagPrefix))
            {
                string oldControlName = attrMap[ControlTagPrefix] + ":" + attrMap[ControlTagName];
                string newControlName = Path.GetFileNameWithoutExtension(attrMap[ControlSourceFile]);
                _registeredUserControls.UserControlRulesMap[oldControlName] = new UserControlConverter(newControlName);

                //_registeredUserControls.UserControlRulesMap.Add(oldControlName, new UserControlConverter(newControlName));

                string namespaceName = FilePathHelper.FilePathToNamespace(attrMap[ControlSourceFile], projectName);
                string content = $"@using {namespaceName}";
                migratedDirectives.Add(new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective,
                    content));
            }
            else
            {
                migratedDirectives.Add(new DirectiveMigrationResult(DirectiveMigrationResultType.Comment,
                    IncorrectRegisterDirectiveWarning));
            }
            
            return migratedDirectives;
        }
    }
}
