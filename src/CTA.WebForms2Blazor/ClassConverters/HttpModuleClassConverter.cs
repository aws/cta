using System;
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
    public class HttpModuleClassConverter : ClassConverter
    {
        private LifecycleManagerService _lifecycleManager;

        public HttpModuleClassConverter(
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

        public override async Task<FileInformation> MigrateClassAsync()
        {
            // TODO:
            // Get all methods
            // Separate out lifecycle methods
            // If there's more than one edit names
            // Register new middlewares

            var className = _originalDeclarationSyntax.Identifier.ToString();
            var namespaceName = _originalClassSymbol.ContainingNamespace.ToDisplayString();
            var requiredUsings = CodeSyntaxHelper.BuildUsingStatements(_sourceFileSemanticModel
                .GetNamespacesReferencedByType(_originalDeclarationSyntax)
                .Select(namespaceSymbol => namespaceSymbol.ToDisplayString()));

            // Make this call once now so we don't have to keep doing it later
            var originalDescendantNodes = _originalDeclarationSyntax.DescendantNodes();
            // Classify methods and store results as tuple so we don't have to keep recalculating it
            var classifiedMethods = originalDescendantNodes.OfType<MethodDeclarationSyntax>()
                .Select(method => (method, LifecycleManagerService.CheckMethodApplicationLifecycleHook(method)));

            var sharedMethods = classifiedMethods.Where(methodTuple => methodTuple.Item2 == null).Select(methodTuple => methodTuple.Item1);
            var lifecycleTuples = classifiedMethods.Where(methodTuple => methodTuple.Item2 != null) as IEnumerable<(MethodDeclarationSyntax, WebFormsAppLifecycleEvent)>;
            var fileInfoCollection = new List<FileInformation>();

            if (lifecycleTuples.Count() > 1)
            {
                foreach (var lifecycleTuple in lifecycleTuples)
                {
                    var newClassName = className + lifecycleTuple.Item2.ToString();
                    _lifecycleManager.RegisterMiddlewareClass(lifecycleTuple.Item2, newClassName, namespaceName, className, true);
                    var middlewareClassDeclaration = GetNewMiddlewareClass(lifecycleTuple.Item1, newClassName);
                    var newRelativePath = Path.Combine(Constants.MiddlewareDirectoryName, FilePathHelper.AlterFileName(_relativePath, newFileName: newClassName));
                    var namespaceNode = CodeSyntaxHelper.BuildNamespace(namespaceName, middlewareClassDeclaration);
                    var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, requiredUsings);
                    fileInfoCollection.Add(new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText)));
                }
            }
            else if (lifecycleTuples.Any())
            {
                var lifecycleTuple = lifecycleTuples.Single();
                _lifecycleManager.RegisterMiddlewareClass(lifecycleTuple.Item2, className, namespaceName, className, false);
                var middlewareClassDeclaration = GetNewMiddlewareClass(lifecycleTuple.Item1, className);
                var newRelativePath = Path.Combine(Constants.MiddlewareDirectoryName, _relativePath);
                var namespaceNode = CodeSyntaxHelper.BuildNamespace(namespaceName, middlewareClassDeclaration);
                var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, requiredUsings);
                fileInfoCollection.Add(new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText)));
            }
            else
            {
                // TOOD: Attach comment on method is null

                var middlewareClassDeclaration = GetNewMiddlewareClass(null, className);
                var newRelativePath = Path.Combine(Constants.MiddlewareDirectoryName, _relativePath);
                var namespaceNode = CodeSyntaxHelper.BuildNamespace(namespaceName, middlewareClassDeclaration);
                var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, requiredUsings);
                fileInfoCollection.Add(new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText)));
            }

            // By this point all new middleware has been registered
            _lifecycleManager.NotifyMiddlewareSourceProcessed();

            // TODO:
            // Build new middleware classes
            // Add classes to namespaces
            // Get fileTexts
            // Return them

            DoCleanUp();

            // Http modules are turned into middleware and so we use a new middleware directory
            // TODO: Potentially remove certain folders from beginning of relative path
            return new FileInformation(Path.Combine(Constants.MiddlewareDirectoryName, _relativePath), Encoding.UTF8.GetBytes(sourceClassComponents.FileText));
        }

        private ClassDeclarationSyntax GetNewMiddlewareClass(MethodDeclarationSyntax methodDeclaration, string middlewareName)
        {
            throw new NotImplementedException();
        }
    }
}
