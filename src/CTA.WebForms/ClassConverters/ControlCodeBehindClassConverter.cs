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
    public class ControlCodeBehindClassConverter : ClassConverter
    {
        private const string ActionName = "ControlCodeBehindClassConverter";
        private WebFormMetricContext _metricsContext;
        public ControlCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager,
            WebFormMetricContext metricsContext)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            _metricsContext = metricsContext;
            // TODO: Register with the necessary services
        }

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();
            _metricsContext.CollectActionMetrics(WebFormsActionType.ClassConversion, ActionName);
            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            //var namespaceNames = requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.ToDisplayString()).Append(Constants.BlazorComponentsNamespace);

            var requiredNamespaces = _sourceFileSemanticModel.GetOriginalUsingNamespaces().Append(Constants.BlazorComponentsNamespace);
            requiredNamespaces = CodeSyntaxHelper.RemoveFrameworkUsings(requiredNamespaces);

            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces);

            var modifiedClass = ((ClassDeclarationSyntax)_originalDeclarationSyntax)
                // Remove outdated base type references
                // TODO: Scan and remove specific base types in the future
                .ClearBaseTypes()
                // ComponentBase base class is added because user controls become
                // normal razor components
                .AddBaseType(Constants.ComponentBaseClass);

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace?.ToDisplayString(), modifiedClass);

            DoCleanUp();
            LogEnd();

            var newRelativePath = GetNewRelativePath();
            var fileContent = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, usingStatements);
            var result = new[] { new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileContent)) };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }

        private string GetNewRelativePath()
        {
            // TODO: Potentially remove certain folders from beginning of relative path
            var newRelativePath = FilePathHelper.AlterFileName(_relativePath,
                oldExtension: Constants.ControlCodeBehindExtension,
                newExtension: Constants.RazorCodeBehindFileExtension);
            
            return FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.RazorComponentDirectoryName, newRelativePath));
        }
    }
}
