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
    public class HttpHandlerClassConverter : ClassConverter
    {
        private const string ProcessRequestDiscovery = "ProcessRequest method";
        private const string InvokePopulationOperation = "middleware Invoke method population";

        private LifecycleManagerService _lifecycleManager;

        public HttpHandlerClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            LifecycleManagerService lifecycleManager,
            TaskManagerService taskManager)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            _lifecycleManager = lifecycleManager;
            _lifecycleManager.NotifyExpectedMiddlewareSource();
        }

        public override async Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            var className = _originalDeclarationSyntax.Identifier.ToString();
            var namespaceName = _originalClassSymbol.ContainingNamespace.ToDisplayString();
            var requiredNamespaceNames = _sourceFileSemanticModel
                .GetNamespacesReferencedByType(_originalDeclarationSyntax)
                .Select(namespaceSymbol => namespaceSymbol.ToDisplayString());

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

            var middlewareClassDeclaration = MiddlewareSyntaxHelper.ConstructMiddlewareClass(
                middlewareClassName: className,
                shouldContinueAfterInvoke: false,
                constructorAdditionalStatements: originalDescendantNodes.OfType<ConstructorDeclarationSyntax>().FirstOrDefault()?.Body?.Statements,
                preHandleStatements: preHandleStatements,
                additionalFieldDeclarations: originalDescendantNodes.OfType<FieldDeclarationSyntax>(),
                additionalPropertyDeclarations: originalDescendantNodes.OfType<PropertyDeclarationSyntax>(),
                additionalMethodDeclarations: keepableMethods);

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(namespaceName, middlewareClassDeclaration);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, CodeSyntaxHelper.BuildUsingStatements(requiredNamespaceNames));

            DoCleanUp();
            LogEnd();

            // Http modules are turned into middleware and so we use a new middleware directory
            var newRelativePath = Path.Combine(Constants.MiddlewareDirectoryName, FilePathHelper.AlterFileName(_relativePath, newFileName: className));
            // TODO: Potentially remove certain folders from beginning of relative path
            return new[] { new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText)) };
        }
    }
}
