using System;
using System.Collections;
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
        public const string SourceAttr = "Src";

        // Decided to use a universal attribute map because many of the attributes seem to be shared between
        // directives and using attribute allow lists by directive seemed like a better solution
        // Func takes in a string array to allow for variable number of string inputs,
        // which may be required for different attributes
        public static IDictionary<string, Func<string[], IEnumerable<DirectiveMigrationResult>>> AttributeMap
        {
            get
            {
                // NOTE: Directive attributes appear to be case-insensitive
                return new Dictionary<string, Func<string[], IEnumerable<DirectiveMigrationResult>>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    [MasterPageFileAttr] = input =>
                    {
                        // TODO: Properly extract layout class name using file pointed to by attrValue, likely need to use
                        // a service for this, problem is that attrValue can be a full path, partial path, or relative path
                        // and so building redundant retrieval may be more effort than its worth at the moment
                        string attrValue = input[0];
                        var layoutClassName = Path.GetFileNameWithoutExtension(attrValue.RemoveOuterQuotes());

                        return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, $"@layout {layoutClassName}") };
                    },
                    [InheritsAttr] = input =>
                    {
                        // TODO: Verify if inherited base class is still a valid base class (using CTA/Codelyzer?) and
                        // return Enumerable.Empty<DirectiveMigrationResult>() if it is no longer valid
                        string attrValue = input[0];
                         return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, $"@inherits {attrValue.RemoveOuterQuotes()}") };
                    },
                    // No longer sure if this is supported in .NET 5
                    // TODO: Find some other way to deal with this attribute
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
                    [CodeBehind] = input =>
                    {
                        // TODO: If code behind file name is different from expected value we should notify a service here
                        // Functionality not yet implemented in the DirectiveConverters
                        // string attrValue = input[0];
                        // string expected = input[1];
                        // if (attrValue != expected)
                        // {
                        //     return new[]
                        //     {
                        //         new DirectiveMigrationResult(DirectiveMigrationResultType.Comment,
                        //             $"<!-- CodeBehind name {attrValue} does not match expected name {expected}-->")
                        //     };
                        // }
                        return Enumerable.Empty<DirectiveMigrationResult>();
                    },
                    [ClassNameAttr] = input =>
                    {
                        // TODO: If class name is different from expected value we should notify a service here
                        return Enumerable.Empty<DirectiveMigrationResult>();
                    },
                    // This doesn't need to be converted or have anything to be converted to so we have it return nothing
                    [LanguageAttr] = input => Enumerable.Empty<DirectiveMigrationResult>(),
                    [SourceAttr] = input =>
                    {
                        // TODO: Verify if inherited base class is still a valid base class (using CTA/Codelyzer?) and
                        // return Enumerable.Empty<DirectiveMigrationResult>() if it is no longer valid
                        string attrValue = input[0];
                        return new[]
                        {
                            new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective,
                                $"@inherits {attrValue.RemoveOuterQuotes()}")
                        };
                    }
                };
            }
        }
    }
}
