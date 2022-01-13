using System.Collections.Generic;
using System.IO;

namespace CTA.WebForms.DirectiveConverters
{
    public class PageDirectiveConverter : DirectiveConverter
    {
        private protected override IEnumerable<string> AttributeAllowList
        {
            get
            {
                return new[]
                {
                    UniversalDirectiveAttributeMap.MasterPageFileAttr,
                    UniversalDirectiveAttributeMap.InheritsAttr,
                    UniversalDirectiveAttributeMap.CodeBehind,
                    UniversalDirectiveAttributeMap.ClassNameAttr,
                    UniversalDirectiveAttributeMap.LanguageAttr
                };
            }
        }

        private protected override IEnumerable<DirectiveMigrationResult> GetMigratedDirectives(string directiveName, string originalFilePath)
        {
            // TODO: Retrieve page route from routing service and use it populate pageRoute
            string pageRoute;

            // TODO: Perform this check in a more sophisticated way
            if (originalFilePath.Equals(Constants.DefaultHomePagePath))
            {
                pageRoute = Constants.RouteSeparator.ToString();
            }
            else
            {
                var directory = Path.GetDirectoryName(originalFilePath);
                var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
                var modifiedPath = directory != null ? Path.Combine(directory, fileName) : fileName;

                pageRoute = Constants.RouteSeparator + modifiedPath.Replace(Path.DirectorySeparatorChar, Constants.RouteSeparator);
            }

            return new[] { new DirectiveMigrationResult(DirectiveMigrationResultType.GeneralDirective, string.Format(Constants.RazorPageDirective, pageRoute)) };
        }
    }
}
