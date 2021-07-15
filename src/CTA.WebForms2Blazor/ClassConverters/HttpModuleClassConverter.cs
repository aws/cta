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
        private const string LifecycleEventDiscovery = "lifecycle hook method";
        private const string InvokePopulationOperation = "middleware Invoke method population";

        private IEnumerable<UsingDirectiveSyntax> _requiredUsings;
        private IEnumerable<MethodDeclarationSyntax> _sharedMethods;
        private IEnumerable<FieldDeclarationSyntax> _sharedFields;
        private IEnumerable<PropertyDeclarationSyntax> _sharedProperties;
        private IEnumerable<StatementSyntax> _constructorStatements;
        private LifecycleManagerService _lifecycleManager;

        private string _originalClassName;
        private string _namespaceName;

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

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            _originalClassName = _originalDeclarationSyntax.Identifier.ToString();
            _namespaceName = _originalClassSymbol.ContainingNamespace.ToDisplayString();
            _requiredUsings = CodeSyntaxHelper.BuildUsingStatements(_sourceFileSemanticModel
                .GetNamespacesReferencedByType(_originalDeclarationSyntax)
                .Select(namespaceSymbol => namespaceSymbol.ToDisplayString()));

            // Make this call once now so we don't have to keep doing it later
            var originalDescendantNodes = _originalDeclarationSyntax.DescendantNodes();
            // Classify methods and store results as tuple so we don't have to keep recalculating it
            var classifiedMethods = originalDescendantNodes.OfType<MethodDeclarationSyntax>()
                // NOTE: This is not the proper way to check application lifecycle hooks in http modules,
                // in reality modules must manually add their methods to the application builder Init method
                // parameter as event handlers, parsing this extracting the event the methods correspond to
                // would be costly time-wise so instead we rely on the assumption that they follow proper
                // naming conventions
                .Select(method => (method, LifecycleManagerService.CheckMethodApplicationLifecycleHook(method)));

            // These shared features will be a part of all middleware classes generated because there's no
            // easy way to share scope between them or split up what each one requires while maintaining functionality,
            // best to let the developer handle this
            // NOTE: We want to remove the init method here as it is no longer used, in the future we want to scan this
            // for true application lifecycle event handlers rather than relying on convention
            _sharedMethods = classifiedMethods.Where(methodTuple => methodTuple.Item2 == null && !IsInitMethod(methodTuple.Item1))
                .Select(methodTuple => methodTuple.Item1);
            _sharedFields = originalDescendantNodes.OfType<FieldDeclarationSyntax>();
            _sharedProperties = originalDescendantNodes.OfType<PropertyDeclarationSyntax>();
            _constructorStatements = originalDescendantNodes.OfType<ConstructorDeclarationSyntax>().FirstOrDefault()?.Body?.Statements;

            var lifecycleTuples = classifiedMethods.Where(methodTuple => methodTuple.Item2 != null);
            var fileInfoCollection = new List<FileInformation>();

            // When a handler implements multiple events copy all shared elements into new
            // middleware classes for each event
            if (lifecycleTuples.Count() > 1)
            {
                foreach (var lifecycleTuple in lifecycleTuples)
                {
                    var newClassName = _originalClassName + lifecycleTuple.Item2.ToString();
                    _lifecycleManager.RegisterMiddlewareClass((WebFormsAppLifecycleEvent)lifecycleTuple.Item2, newClassName, _namespaceName, _originalClassName, true);
                    fileInfoCollection.Add(GetNewMiddlewareFileInformation(
                        lifecycleTuple.Item1,
                        newClassName,
                        LifecycleManagerService.ContentIsPreHandle((WebFormsAppLifecycleEvent)lifecycleTuple.Item2),
                        _originalClassName));
                }
            }
            else if (lifecycleTuples.Any())
            {
                var lifecycleTuple = lifecycleTuples.Single();
                _lifecycleManager.RegisterMiddlewareClass((WebFormsAppLifecycleEvent)lifecycleTuple.Item2, _originalClassName, _namespaceName, _originalClassName, false);
                fileInfoCollection.Add(GetNewMiddlewareFileInformation(
                    lifecycleTuple.Item1,
                    _originalClassName,
                    LifecycleManagerService.ContentIsPreHandle((WebFormsAppLifecycleEvent)lifecycleTuple.Item2)));
            }
            else
            {
                fileInfoCollection.Add(GetNewMiddlewareFileInformation(null, _originalClassName));
            }

            // By this point all new middleware has been registered
            _lifecycleManager.NotifyMiddlewareSourceProcessed();

            DoCleanUp();
            LogEnd();

            // TODO: Potentially remove certain folders from beginning of relative path
            return Task.FromResult((IEnumerable<FileInformation>)fileInfoCollection);
        }

        private FileInformation GetNewMiddlewareFileInformation(MethodDeclarationSyntax methodDeclaration, string middlewareName, bool isPreHandle = false, string originClass = null)
        {
            IEnumerable<StatementSyntax> invokeStatements;

            if (methodDeclaration == null)
            {
                invokeStatements = new[]
                {
                    CodeSyntaxHelper.GetBlankLine().AddComment(string.Format(Constants.IdentificationFailureCommentTemplate, LifecycleEventDiscovery, InvokePopulationOperation))
                };
            }
            else
            {
                invokeStatements = methodDeclaration.Body.Statements;
            }

            var classDeclaration = MiddlewareSyntaxHelper.ConstructMiddlewareClass(
                middlewareClassName: middlewareName,
                constructorAdditionalStatements: _constructorStatements,
                preHandleStatements: isPreHandle ? invokeStatements : null,
                postHandleStatements: isPreHandle ? null : invokeStatements,
                additionalFieldDeclarations: _sharedFields,
                additionalPropertyDeclarations: _sharedProperties,
                additionalMethodDeclarations: _sharedMethods);

            if (!string.IsNullOrEmpty(originClass))
            {
                // A split http module likely requires heavy manual modification,
                // make sure they are aware of this and where the code came from
                classDeclaration = classDeclaration.AddComment(new[] {
                    Constants.HeavyModificationNecessaryComment,
                    string.Format(Constants.ClassSplitCommentTemplate, originClass)
                }, lineCharacterSoftLimit: Constants.DefaultCommentLineCharacterLimit);
            }

            // Http modules are turned into middleware and so we use a new middleware directory
            var newRelativePath = Path.Combine(Constants.MiddlewareDirectoryName, FilePathHelper.AlterFileName(_relativePath, newFileName: middlewareName));
            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_namespaceName, classDeclaration);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, _requiredUsings);

            return new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText));
        }

        private bool IsInitMethod(MethodDeclarationSyntax methodDeclaration)
        {
            var paramList = methodDeclaration.ParameterList.Parameters;
            var firstParam = paramList.FirstOrDefault();

            // Expected global base class is HttpApplication which is where event handlers for application
            // lifecycle are registered
            return paramList.Count() == 1 && firstParam.Type.ToString().Equals(Constants.ExpectedGlobalBaseClass)
                && methodDeclaration.Identifier.ToString().Equals(Constants.InitMethodName);
        }
    }
}
