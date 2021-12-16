using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Models;
using CTA.WebForms.Extensions;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Helpers;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.ClassConverters
{
    public class MasterPageCodeBehindClassConverter : ClassConverter
    {
        private const string ActionName = "MasterPageCodeBehindClassConverter";
        private WebFormMetricContext _metricsContext;

        public MasterPageCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager,
            WebFormMetricContext metricsContext)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax,
                originalClassSymbol, taskManager)
        {
            _metricsContext = metricsContext;
        }

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            _metricsContext.CollectActionMetrics(WebFormsActionType.ClassConversion, ActionName);
            var containingNamespace = _originalClassSymbol.ContainingNamespace?.ToDisplayString();

            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            //var namespaceNames = requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.ToDisplayString()).Append(Constants.BlazorComponentsNamespace);

            var namespaceNames = _sourceFileSemanticModel.GetOriginalUsingNamespaces().Append(Constants.BlazorComponentsNamespace);
            namespaceNames = CodeSyntaxHelper.RemoveFrameworkUsings(namespaceNames);
            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(namespaceNames);

            var modifiedClass = ((ClassDeclarationSyntax)_originalDeclarationSyntax)
                // Remove outdated base type references
                // TODO: Scan and remove specific base types in the future
                .ClearBaseTypes()
                // LayoutComponentBase base class is required to use in @layout directive
                .AddBaseType(Constants.LayoutComponentBaseClass);

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(containingNamespace, modifiedClass);

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

            return FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.RazorLayoutDirectoryName, newRelativePath));
        }
    }
}
