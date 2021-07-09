using System;
using System.Collections.Generic;
using System.IO;
using CTA.WebForms2Blazor.Extensions;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class MasterDirectiveConverter : DirectiveConverter
    {
        private protected override IEnumerable<string> AttributeAllowList
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
            return new DirectiveMigrationResult(DirectiveMigrationResultType.Directive, "@inherits LayoutComponentBase");
        }
    }
}
