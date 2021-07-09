using System;
using System.Collections.Generic;
using System.IO;
using CTA.WebForms2Blazor.Extensions;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class PageDirectiveConverter : DirectiveConverter
    {
        private const string UnknownPagePlaceHolderText = "Unkown_page_route";

        private protected override IEnumerable<string> AttributeWhitelist
        {
            get
            {
                return new[]
                {
                    UniversalDirectiveAttributeMap.MasterPageFileAttr,
                    UniversalDirectiveAttributeMap.InheritsAttr,
                    UniversalDirectiveAttributeMap.TitleAttr,
                    UniversalDirectiveAttributeMap.CodeBehind,
                    UniversalDirectiveAttributeMap.ClassNameAttr,
                    UniversalDirectiveAttributeMap.LanguageAttr
                };
            }
        }

        private protected override DirectiveMigrationResult GetMigratedDirective(string directiveName)
        {
            var pageRoute = UnknownPagePlaceHolderText;

            // TODO: Retrieve page route from routing service and use it populate pageRoute

            return new DirectiveMigrationResult(DirectiveMigrationResultType.Directive, $"@page \"{pageRoute}\"");
        }
    }
}
