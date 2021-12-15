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
    public class HttpHandlerClassConverter : ClassConverter
    {
        private const string ProcessRequestDiscovery = "ProcessRequest method";
        private const string InvokePopulationOperation = "middleware Invoke method population";
        private const string ActionName = "HttpHandlerClassConverter";
        private WebFormMetricContext _metricsContext;

        private LifecycleManagerService _lifecycleManager;

        public HttpHandlerClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            LifecycleManagerService lifecycleManager,
            TaskManagerService taskManager,
            WebFormMetricContext metricsContext)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            _lifecycleManager = lifecycleManager;
            _lifecycleManager.NotifyExpectedMiddlewareSource();
            _metricsContext = metricsContext;
        }

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            _metricsContext.CollectActionMetrics(WebFormsActionType.ClassConversion, ActionName);
            var className = _originalDeclarationSyntax.Identifier.ToString();
            var namespaceName = _originalClassSymbol.ContainingNamespace?.ToDisplayString();

            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //var requiredNamespaceNames = _sourceFileSemanticModel
            //    .GetNamespacesReferencedByType(_originalDeclarationSyntax)
            //    .Select(namespaceSymbol => namespaceSymbol.ToDisplayString());

            var requiredNamespaceNames = _sourceFileSemanticModel.GetOriginalUsingNamespaces()
                .Union(MiddlewareSyntaxHelper.RequiredNamespaces);
            requiredNamespaceNames = CodeSyntaxHelper.RemoveFrameworkUsings(requiredNamespaceNames);

            // Make this call once now so we don't have to keep doing it later
            var originalDescendantNodes = _originalDeclarationSyntax.DescendantNodes();
            var keepableMethods = originalDescendantNodes.OfType<MethodDeclarationSyntax>();

            var processRequestMethod = keepableMethods.Where(method => LifecycleManagerService.IsProcessRequestMethod(method)).SingleOrDefault();
            IEnumerable<StatementSyntax> preHandleStatements;

            if (processRequestMethod != null)
            {
                preHandleStatements = processRequestMethod.Body.Statements.AddComment(string.Format(Constants.CodeOriginCommentTemplate, Constants.ProcessRequestMethodName));
                keepableMethods = keepableMethods.Where(method => !method.IsEquivalentTo(processRequestMethod));
                _lifecycleManager.RegisterMiddlewareClass(WebFormsAppLifecycleEvent.RequestHandlerExecute, className, namespaceName, className, false);
            }
            else
            {
                preHandleStatements = new[]
                {
                    CodeSyntaxHelper.GetBlankLine().AddComment(string.Format(Constants.IdentificationFailureCommentTemplate, ProcessRequestDiscovery, InvokePopulationOperation))
                };
            }

            // We have completed any possible registration by this point
            _lifecycleManager.NotifyMiddlewareSourceProcessed();

            var fileText = string.Empty;

            try
            {
                var middlewareClassDeclaration = MiddlewareSyntaxHelper.ConstructMiddlewareClass(
                    middlewareClassName: className,
                    shouldContinueAfterInvoke: false,
                    constructorAdditionalStatements: originalDescendantNodes.OfType<ConstructorDeclarationSyntax>().FirstOrDefault()?.Body?.Statements,
                    preHandleStatements: preHandleStatements,
                    additionalFieldDeclarations: originalDescendantNodes.OfType<FieldDeclarationSyntax>(),
                    additionalPropertyDeclarations: originalDescendantNodes.OfType<PropertyDeclarationSyntax>(),
                    additionalMethodDeclarations: keepableMethods);

                var namespaceNode = CodeSyntaxHelper.BuildNamespace(namespaceName, middlewareClassDeclaration);
                fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, CodeSyntaxHelper.BuildUsingStatements(requiredNamespaceNames));
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to construct new HttpHandler file content from {OriginalClassName} class at {_fullPath}");
            }

            DoCleanUp();
            LogEnd();

            // Http modules are turned into middleware and so we use a new middleware directory
            var newRelativePath = FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.MiddlewareDirectoryName, FilePathHelper.AlterFileName(_relativePath, newFileName: className)));
            // TODO: Potentially remove certain folders from beginning of relative path
            var result = new[] { new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText)) };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }
    }
}
