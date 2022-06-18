using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Config;
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
        private readonly CodeBehindReferenceLinkerService _codeBehindLinkerService;
        private WebFormMetricContext _metricsContext;

        public ControlCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager,
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            WebFormMetricContext metricsContext)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            _codeBehindLinkerService = codeBehindLinkerService;
            _metricsContext = metricsContext;
            // Need to register the code behind at converter creation, before migration logic in
            // view converters need that information
            _codeBehindLinkerService.RegisterCodeBehindFile(Path.ChangeExtension(FullPath, null));
        }

        public override async Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();
            _metricsContext.CollectActionMetrics(WebFormsActionType.ClassConversion, ActionName);
            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            //var namespaceNames = requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.ToDisplayString()).Append(Constants.BlazorComponentsNamespace);

            var modifiedClass = await DoTagCodeBehindConversionsAsync(_originalDeclarationSyntax as ClassDeclarationSyntax);

            var requiredNamespaces = _sourceFileSemanticModel.GetOriginalUsingNamespaces().Append(Constants.BlazorComponentsNamespace);
            requiredNamespaces = CodeSyntaxHelper.RemoveFrameworkUsings(requiredNamespaces);

            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces);

            modifiedClass = modifiedClass
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

            return result;
        }

        /// <summary>
        /// Handles conversion of references to controls in the current code behind file.
        /// </summary>
        /// <param name="classDeclaration">The class declaration within which to convert references.</param>
        /// <returns>The modified class declaration.</returns>
        private async Task<ClassDeclarationSyntax> DoTagCodeBehindConversionsAsync(ClassDeclarationSyntax classDeclaration)
        {
            var viewFilePath = Path.ChangeExtension(FullPath, null);

            try
            {
                return await _taskManager.ManagedRun(_taskId, (token) =>
                    _codeBehindLinkerService.ExecuteTagCodeBehindHandlersAsync(viewFilePath, _sourceFileSemanticModel, classDeclaration, token));
            }
            catch (OperationCanceledException e)
            {
                LogHelper.LogError(e, string.Format(
                    Constants.CaneledServiceCallLogTemplate,
                    Rules.Config.Constants.WebFormsErrorTag,
                    GetType().Name,
                    nameof(CodeBehindReferenceLinkerService),
                    nameof(CodeBehindReferenceLinkerService.ExecuteTagCodeBehindHandlersAsync)));
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to do tag code behind conversions " +
                    $"for file at path {FullPath}");
            }

            return classDeclaration;
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
