using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.WebForms2Blazor.Extensions;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public static class UniversalDirectiveAttributeMap
    {
        public const string MasterPageFileAttr = "MasterPageFile";
        public const string InheritsAttr = "Inherits";
        // public const string TitleAttr = "Title";
        public const string CodeBehind = "CodeBehind";
        public const string ClassNameAttr = "ClassName";
        public const string LanguageAttr = "Language";

        // Decided to use a universal attribute map because many of the attributes seem to be shared between
        // directives and using attribute allow lists by directive seemed like a better solution
        public static IDictionary<string, Func<string, IEnumerable<DirectiveMigrationResult>>> AttributeMap
        {
            get
            {
                // NOTE: Directive attributes appear to be case-insensitive
                return new Dictionary<string, Func<string, IEnumerable<DirectiveMigrationResult>>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    [MasterPageFileAttr] = attrValue =>
                    {
                        // TODO: Properly extract layout class name using file pointed to by attrValue, likely need to use
                        // a service for this, problem is that attrValue can be a full path, partial path, or relative path
                        // and so building redundant retrieval may be more effort than its worth at the moment
                        var layoutClassName = Path.GetFileNameWithoutExtension(attrValue.RemoveOuterQuotes());

                        return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, $"@layout {layoutClassName}") };
                    },
                    [InheritsAttr] = attrValue =>
                    {
                        // TODO: Verify if inherited base class is still a valid base class (using CTA/Codelyzer?) and
                        // return Enumerable.Empty<DirectiveMigrationResult>() if it is no longer valid
                         return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, $"@inherits {attrValue.RemoveOuterQuotes()}") };
                    },
                    // No longer sure if this is supported in .NET 5
                    // [TitleAttr] = attrValue =>
                    // {
                    //     var usingDirective = "@using Microsoft.AspNetCore.Components.Web.Extensions.Head";
                    //     var titleTag = $"<Title value={attrValue} />";

                    //     return new[]
                    //     {
                    //         new DirectiveMigrationResult(DirectiveMigrationResultType.UsingDirective, usingDirective),
                    //         new DirectiveMigrationResult(DirectiveMigrationResultType.HTMLNode, titleTag)
                    //     };
                    // },
                    [CodeBehind] = attrValue =>
                    {
                        // TODO: If code behind file name is different from expected value we should notify a service here
                        return Enumerable.Empty<DirectiveMigrationResult>();
                    },
                    [ClassNameAttr] = attrValue =>
                    {
                        // TODO: If class name is different from expected value we should notify a service here
                        return Enumerable.Empty<DirectiveMigrationResult>();
                    },
                    // This doesn't need to be converted or have anything to be converted to so we have it return nothing
                    [LanguageAttr] = attrvalue => Enumerable.Empty<DirectiveMigrationResult>(),
                };
            }
        }
    }
}
