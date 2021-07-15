using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Extensions;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class MasterPageCodeBehindClassConverter : ClassConverter
    {
        public MasterPageCodeBehindClassConverter(
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

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.ToDisplayString()));

            var modifiedClass = ((ClassDeclarationSyntax)_originalDeclarationSyntax)
                // Remove outdated base type references
                // TODO: Scan and remove specific base types in the future
                .ClearBaseTypes()
                // LayoutComponentBase base class is required to use in @layout directive
                .AddBaseType(Constants.LayoutComponentBaseClass);

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace.Name, modifiedClass);

            DoCleanUp();
            LogEnd();

            var result = new[] { new FileInformation(GetNewRelativePath(), Encoding.UTF8.GetBytes(CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, usingStatements))) };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }

        private string GetNewRelativePath()
        {
            // TODO: Potentially remove certain folders from beginning of relative path
            var newRelativePath = FilePathHelper.AlterFileName(_relativePath,
                oldExtension: Constants.MasterPageCodeBehindExtension,
                newExtension: Constants.RazorCodeBehindFileExtension);

            return Path.Combine(Constants.RazorLayoutDirectoryName, newRelativePath);
        }
    }
}
