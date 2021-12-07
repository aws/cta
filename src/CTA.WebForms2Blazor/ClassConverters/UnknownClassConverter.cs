using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Extensions;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class UnknownClassConverter : ClassConverter
    {
        private const string ActionName = "UnknownClassConverter";
        private WebFormMetricContext _metricsContext;
        public UnknownClassConverter(
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

            _metricsContext.CollectClassConversionMetrics(ActionName);
            // NOTE: We could just read the file from the disk and retrieve the bytes like
            // that but instead I opted to "rebuild" the type in case we wanted to add comments
            // or something else to these undefined code files, most likely though we may still
            // want to scan parts of these files and remove/alter/take note of certain lines/info

            var newRelativePath = FilePathHelper.RemoveDuplicateDirectories(FilePathHelper.AlterFileName(_relativePath, newFileName: _originalClassSymbol.Name));

            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //var sourceClassComponents = GetSourceClassComponents();

            var namespaceNames = _sourceFileSemanticModel.GetOriginalUsingNamespaces().Append(Constants.BlazorComponentsNamespace);
            namespaceNames = CodeSyntaxHelper.RemoveFrameworkUsings(namespaceNames);
            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(namespaceNames);
            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace.ToDisplayString(), _originalDeclarationSyntax);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, usingStatements);

            DoCleanUp();
            LogEnd();

            var result = new[] { new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText)) };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }
    }
}
