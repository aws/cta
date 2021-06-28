using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class UnknownClassConverter : ClassConverter
    {
        public UnknownClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol)
        {
            // TODO: Register with the necessary services
        }

        public override async Task<FileInformation> MigrateClassAsync()
        {
            // NOTE: We could just read the file from the disk and retrieve the bytes like
            // that but instead I opted to "rebuild" the type in case we wanted to add comments
            // or something else to these undefined code files, most likely though we may still
            // want to scan parts of these files and remove/alter/take note of certain lines/info
            var sourceClassComponents = GetSourceClassComponents();

            return new FileInformation(_relativePath, Encoding.UTF8.GetBytes(sourceClassComponents.FileText));
        }
    }
}
