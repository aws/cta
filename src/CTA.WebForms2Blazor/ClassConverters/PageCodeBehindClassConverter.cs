using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.IO;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class PageCodeBehindClassConverter : ClassConverter
    {
        public PageCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            // TODO: Register with the necessary services
        }

        public override async Task<FileInformation> MigrateClassAsync()
        {
            // NOTE: For now we make no code modifications, just to be
            // ready for the demo and produces files
            // TODO: Modify namespace according to new relative path? Will,
            // need to track a change like that in the reference manager and
            // modify using statements in other files, determing all namespace
            // changes before re-assembling new using statement collection will
            // make this possible
            var sourceClassComponents = GetSourceClassComponents();

            // Retrieve methods that are event handlers
            // Extract statements and add to component lifecycle

            // _originalDeclarationSyntax
            // remove node
            // add members
            // Add componentbase class
            // Add Disposable if needed

            DoCleanUp();

            return new FileInformation(GetNewRelativePath(), Encoding.UTF8.GetBytes(sourceClassComponents.FileText));
        }

        private string GetNewRelativePath()
        {
            // TODO: Potentially remove certain folders from beginning of relative path
            var newRelativePath = FilePathHelper.AlterFileName(_relativePath,
                oldExtension: Constants.PageCodeBehindExtension,
                newExtension: Constants.RazorCodeBehindFileExtension);

            return Path.Combine(Constants.RazorPageDirectoryName, newRelativePath);
        }
    }
}
