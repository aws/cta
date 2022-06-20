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
    public class MasterPageCodeBehindClassConverter : ClassConverter
    {
        private const string ActionName = "MasterPageCodeBehindClassConverter";
        private readonly CodeBehindReferenceLinkerService _codeBehindLinkerService;
        private WebFormMetricContext _metricsContext;

        public MasterPageCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager,
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            WebFormMetricContext metricsContext)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax,
                originalClassSymbol, taskManager)
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
            var containingNamespace = _originalClassSymbol.ContainingNamespace?.ToDisplayString();

            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            //var namespaceNames = requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.ToDisplayString()).Append(Constants.BlazorComponentsNamespace);

            var modifiedClass = await DoTagCodeBehindConversionsAsync(_originalDeclarationSyntax as ClassDeclarationSyntax);

            var namespaceNames = _sourceFileSemanticModel.GetOriginalUsingNamespaces().Append(Constants.BlazorComponentsNamespace);
            namespaceNames = CodeSyntaxHelper.RemoveFrameworkUsings(namespaceNames);
            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(namespaceNames);

            modifiedClass = modifiedClass
                // Remove outdated base type references
                // TODO: Scan and remove specific base types in the future
                .ClearBaseTypes()
                // LayoutComponentBase base class is required to use in @layout directive
                .AddBaseType(Constants.LayoutComponentBaseClass);

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(containingNamespace, modifiedClass);

            DoCleanUp();
            LogEnd();

            var result = new[] { new FileInformation(GetNewRelativePath(), Encoding.UTF8.GetBytes(CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, usingStatements))) };

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
                oldExtension: Constants.MasterPageCodeBehindExtension,
                newExtension: Constants.RazorCodeBehindFileExtension);

            return FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.RazorLayoutDirectoryName, newRelativePath));
        }
    }
}
