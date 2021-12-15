using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.WebForms.ControlConverters;
using CTA.WebForms.Extensions;
using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.ControlHelpers;

namespace CTA.WebForms.DirectiveConverters
{
    public class RegisterDirectiveConverter : DirectiveConverter
    {
        private readonly RegisteredUserControls _registeredUserControls;
        private const string IncorrectRegisterDirectiveWarning = "<!-- Register directive missing TagName or TagPrefix -->";
        private const string FileNameDoesNotContainDirectoryErrorTemplate =
            "<!-- Cannot convert file name to namespace, file path {0} does not have a directory -->";
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
            var attrMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var migratedDirectives = new List<DirectiveMigrationResult>();
            
            var attrMatches = AttributeSplitRegex.Matches(directiveString);
            foreach (Match match in attrMatches)
            {
                var attrName = match.Groups[AttributeNameRegexGroupName].Value;
                var attrValue = match.Groups[AttributeValueRegexGroupName].Value;
                attrMap.Add(attrName, attrValue.RemoveOuterQuotes());
            }

            if (attrMap.ContainsKey(ControlTagName) && attrMap.ContainsKey(ControlTagPrefix))
            {
                var oldControlName = attrMap[ControlTagPrefix] + ":" + attrMap[ControlTagName];
                var newControlName = Path.GetFileNameWithoutExtension(attrMap[ControlSourceFile]);
                _registeredUserControls.UserControlRulesMap[oldControlName] = new UserControlConverter(newControlName);

                //_registeredUserControls.UserControlRulesMap.Add(oldControlName, new UserControlConverter(newControlName));
                var filePath = attrMap[ControlSourceFile];
                var blazorNamespace = FilePathHelper.GetNamespaceFromRelativeFilePath(filePath, projectName);
                
                if(string.IsNullOrEmpty(blazorNamespace))
                {
                    var errorMessageCommentString = string.Format(FileNameDoesNotContainDirectoryErrorTemplate, filePath);
                    var commentedDirectiveString = directiveString.ConvertToRazorComment();
                    var content = Utilities.SeparateStringsWithNewLine(errorMessageCommentString, commentedDirectiveString);

                    migratedDirectives.Add(new DirectiveMigrationResult(DirectiveMigrationResultType.Comment,
                        content));
                }
                else
                {
                    var content = $"@using {blazorNamespace}";
                    migratedDirectives.Add(new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective,
                        content));
                }
            }
            else
            {
                var commentedDirectiveString = directiveString.ConvertToRazorComment();
                var content = Utilities.SeparateStringsWithNewLine(IncorrectRegisterDirectiveWarning, commentedDirectiveString);

                migratedDirectives.Add(new DirectiveMigrationResult(DirectiveMigrationResultType.Comment,
                    content));
            }
            
            return migratedDirectives;
        }
    }
}
