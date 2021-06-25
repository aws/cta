using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.IO;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class MasterPageCodeBehindClassConverter : ClassConverter
    {
        public MasterPageCodeBehindClassConverter(
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
            // NOTE: For now we make no code modifications, just to be
            // ready for the demo and produces files
            var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.Name));
            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace.Name, _originalDeclarationSyntax);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, usingStatements);

            return new FileInformation(GetNewRelativePath(), Encoding.UTF8.GetBytes(fileText));
        }

        private string GetNewRelativePath()
        {
            // TODO: Potentially remove certain folders from beginning of relative path
            var relativePathFolder = Path.GetDirectoryName(_relativePath);
            var oldFileName = Path.GetFileName(_relativePath);
            // Path.GetFileNameWithoutExtension only removes the .cs of .Master.cs
            // and so we have to remove the extension manually
            var baseFileNameLength = oldFileName.Length - Constants.MasterPageCodeBehindExtension.Length;
            // The code behinds are responsible for generating the new razor
            // file and so we replace the extension with .razor
            var newFileName = oldFileName.Substring(0, baseFileNameLength) + Constants.RazorFileExtension;
            return Path.Combine(Constants.RazorLayoutDirectoryName, relativePathFolder, newFileName);
        }
    }
}
