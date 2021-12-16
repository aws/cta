using System.Collections.Generic;

namespace CTA.WebForms.DirectiveConverters
{
    public class MasterDirectiveConverter : DirectiveConverter
    {
        private const string UnknownNamespacePlaceHolderText = "Replace_this_with_code_behind_namespace";

        private protected override IEnumerable<string> AttributeAllowList
        {
            get
            {
                return new[]
                {
                    UniversalDirectiveAttributeMap.MasterPageFileAttr,
                    UniversalDirectiveAttributeMap.CodeBehind,
                    UniversalDirectiveAttributeMap.ClassNameAttr,
                    UniversalDirectiveAttributeMap.LanguageAttr
                };
            }
        }

        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedDirectives(string directiveName, string originalFilePath)
        {
            var layoutNamespace = UnknownNamespacePlaceHolderText;

            // TODO: Retrieve code behind namespace from code behind linker service and use it populate layoutNamespace

            return new[] {
                new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, string.Format(Constants.RazorNamespaceDirective, layoutNamespace)),
                new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, string.Format(Constants.RazorInheritsDirective, Constants.LayoutComponentBaseClass))
            };
        }
    }
}
