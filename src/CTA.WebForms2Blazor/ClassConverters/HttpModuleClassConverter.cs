using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms2Blazor.Extensions;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class HttpModuleClassConverter : ClassConverter
    {
        private const string LifecycleEventDiscovery = "lifecycle hook method";
        private const string InvokePopulationOperation = "middleware Invoke method population";
        private const string ActionName = "HttpHandlerClassConverter";
        private WebFormMetricContext _metricsContext;

        private IEnumerable<UsingDirectiveSyntax> _requiredUsings;
        private IEnumerable<MethodDeclarationSyntax> _sharedMethods;
        private IEnumerable<FieldDeclarationSyntax> _sharedFields;
        private IEnumerable<PropertyDeclarationSyntax> _sharedProperties;
        private IEnumerable<StatementSyntax> _constructorStatements;
        private IEnumerable<(string, WebFormsAppLifecycleEvent)> _moduleEventHandlerExpectedNameTuples;
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
            _originalClassName = _originalDeclarationSyntax.Identifier.ToString();
            _namespaceName = _originalClassSymbol.ContainingNamespace.ToDisplayString();

            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            //_requiredUsings = CodeSyntaxHelper.BuildUsingStatements(_sourceFileSemanticModel
            //    .GetNamespacesReferencedByType(_originalDeclarationSyntax)
            //    .Select(namespaceSymbol => namespaceSymbol.ToDisplayString()));

            var requiredNamespaces = _sourceFileSemanticModel.GetOriginalUsingNamespaces()
                .Union(MiddlewareSyntaxHelper.RequiredNamespaces);
            requiredNamespaces = CodeSyntaxHelper.RemoveFrameworkUsings(requiredNamespaces);
            _requiredUsings = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces);

            // Make this call once now so we don't have to keep doing it later
            var originalDescendantNodes = _originalDeclarationSyntax.DescendantNodes();

            // Process init method first before classifying so that we know what method names to look for
            var initMethod = originalDescendantNodes.OfType<MethodDeclarationSyntax>().Where(method => IsInitMethod(method)).SingleOrDefault();
            // Classify methods and store results as tuple so we don't have to keep recalculating it
            IEnumerable<(MethodDeclarationSyntax, WebFormsAppLifecycleEvent?)> classifiedMethods = null;

            if (initMethod != null)
            {
                try
                {
                    ProcessInitMethod(initMethod);
                    classifiedMethods = originalDescendantNodes.OfType<MethodDeclarationSyntax>().Select(method => (method, CheckModuleEventHandlerMethod(method)));
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Failed to process {OriginalClassName} HttpModule class Init method at {_fullPath}, " +
                        $"as a result lifecycle methods will not be detectable and will not be converted to middleware");
                }
            }

            // NOTE: We want this default value if initMethod is null or if processing the init method fails
            if (classifiedMethods == null)
            {
                classifiedMethods = originalDescendantNodes.OfType<MethodDeclarationSyntax>().Select(method => (method, (WebFormsAppLifecycleEvent?)null));
            }

            // These shared features will be a part of all middleware classes generated because there's no
            // easy way to share scope between them or split up what each one requires while maintaining functionality,
            // best to let the developer handle this
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

                    try
                    {
                        _lifecycleManager.RegisterMiddlewareClass((WebFormsAppLifecycleEvent)lifecycleTuple.Item2, newClassName, _namespaceName, _originalClassName, true);
                        fileInfoCollection.Add(GetNewMiddlewareFileInformation(
                            lifecycleTuple.Item1,
                            newClassName,
                            LifecycleManagerService.ContentIsPreHandle((WebFormsAppLifecycleEvent)lifecycleTuple.Item2),
                            _originalClassName));
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Failed to construct {newClassName} middleware class from {lifecycleTuple.Item2} event " +
                            $" handler in {_originalClassName} class at {_fullPath}");
                    }
                }
            }
            else if (lifecycleTuples.Any())
            {
                var lifecycleTuple = lifecycleTuples.Single();

                try
                {
                    _lifecycleManager.RegisterMiddlewareClass((WebFormsAppLifecycleEvent)lifecycleTuple.Item2, _originalClassName, _namespaceName, _originalClassName, false);
                    fileInfoCollection.Add(GetNewMiddlewareFileInformation(
                        lifecycleTuple.Item1,
                        _originalClassName,
                        LifecycleManagerService.ContentIsPreHandle((WebFormsAppLifecycleEvent)lifecycleTuple.Item2)));
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Failed to construct middleware class from {lifecycleTuple.Item2} event " +
                        $" handler in {_originalClassName} class at {_fullPath}");
                }
            }
            else
            {
                try
                {
                    fileInfoCollection.Add(GetNewMiddlewareFileInformation(null, _originalClassName));
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Failed to construct no-event middleware class from {_originalClassName} class at {_fullPath}");
                }
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
            var newRelativePath = FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.MiddlewareDirectoryName, FilePathHelper.AlterFileName(_relativePath, newFileName: middlewareName)));
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

        private WebFormsAppLifecycleEvent? CheckModuleEventHandlerMethod(MethodDeclarationSyntax methodDeclaration)
        {
            foreach (var expectedNameTuple in _moduleEventHandlerExpectedNameTuples)
            {
                if (methodDeclaration.IsEventHandler(expectedNameTuple.Item1))
                {
                    return expectedNameTuple.Item2;
                }
            }

            return null;
        }

        private void ProcessInitMethod(MethodDeclarationSyntax methodDeclaration)
        {
            // Predicates checked in IsInitMethod() insure that no exceptions will be thrown here
            var appParamName = methodDeclaration.ParameterList.Parameters
                .Where(param => param.Type.ToString().Equals(Constants.ExpectedGlobalBaseClass))
                .Single().Identifier.ToString();

            var assignmentExprs = methodDeclaration.Body.Statements
                .Where(statement => statement.IsKind(SyntaxKind.ExpressionStatement))
                .Select(statement => (statement as ExpressionStatementSyntax).Expression as AssignmentExpressionSyntax)
                .Where(expr => expr?.IsKind(SyntaxKind.AddAssignmentExpression) ?? false);

            var results = new List<(string, WebFormsAppLifecycleEvent)>();
            foreach (var expr in assignmentExprs)
            {
                var lcEvent = LifecycleManagerService.CheckWebFormsLifecycleEventWithPrefix(expr.Left.ToString(), $"{appParamName}.");

                if (lcEvent != null)
                {
                    var objCreationExpr = expr.Right.RemoveSurroundingParentheses() as ObjectCreationExpressionSyntax;

                    if (objCreationExpr != null && objCreationExpr.Type.ToString().Equals(typeof(EventHandler).Name))
                    {
                        try
                        {
                            var arguments = objCreationExpr.ArgumentList.Arguments;
                            var methodNameExpr = arguments.FirstOrDefault()?.Expression;

                            string methodName = null;
                            if (methodNameExpr is MemberAccessExpressionSyntax)
                            {
                                methodName = (methodNameExpr as MemberAccessExpressionSyntax).Name.ToString();
                            }
                            else
                            {
                                methodName = (methodNameExpr as IdentifierNameSyntax).Identifier.ToString();
                            }
                            results.Add((methodName, (WebFormsAppLifecycleEvent)lcEvent));
                        }
                        catch (Exception e)
                        {
                            LogHelper.LogError(e, $"{Constants.WebFormsErrorTag}Failed to retrieve event method name from expression '{objCreationExpr}'" +
                                $"while processing Init method of {OriginalClassName} class");
                        }
                    }
                }
            }

            _moduleEventHandlerExpectedNameTuples = results;
        }
    }
}
