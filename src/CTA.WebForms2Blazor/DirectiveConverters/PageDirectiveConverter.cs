using System.Collections.Generic;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class PageDirectiveConverter : DirectiveConverter
    {
        private const string UnknownPagePlaceHolderText = "Unkown_page_route";

        private protected override IEnumerable<string> AttributeAllowList
        {
            get
            {
                return new[]
                {
                    UniversalDirectiveAttributeMap.MasterPageFileAttr,
                    UniversalDirectiveAttributeMap.InheritsAttr,
                    // UniversalDirectiveAttributeMap.TitleAttr,
                    UniversalDirectiveAttributeMap.CodeBehind,
                    UniversalDirectiveAttributeMap.ClassNameAttr,
                    UniversalDirectiveAttributeMap.LanguageAttr
                };
            }
        }

        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedDirectives(string directiveName)
        {
            var pageRoute = UnknownPagePlaceHolderText;

            // TODO: Retrieve page route from routing service and use it populate pageRoute

            return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, string.Format(Constants.RazorPageDirective, pageRoute)) };
        }
    }
}
