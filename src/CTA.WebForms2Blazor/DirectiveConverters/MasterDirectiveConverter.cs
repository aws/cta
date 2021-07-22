using System.Collections.Generic;

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
            return null;
        }
    }
}
