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
    public class PageCodeBehindClassConverter : ClassConverter
    {
        private IDictionary<BlazorComponentLifecycleEvent, IEnumerable<StatementSyntax>> _newLifecycleLines;

        public PageCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            _newLifecycleLines = new Dictionary<BlazorComponentLifecycleEvent, IEnumerable<StatementSyntax>>();
        }

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            var requiredNamespaceNames = _sourceFileSemanticModel
                .GetNamespacesReferencedByType(_originalDeclarationSyntax)
                .Select(namespaceSymbol => namespaceSymbol.ToDisplayString())
                // This is so we can use ComponentBase base class
                .Append(Constants.BlazorComponentsNamespace);

            var allMethods = _originalDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var currentClassDeclaration = ((ClassDeclarationSyntax)_originalDeclarationSyntax)
                // Need to track methods so modifications can be made one after another
                .TrackNodes(allMethods)
                // Remove outdated base type references
                // TODO: Scan and remove specific base types in the future
                .ClearBaseTypes()
                // ComponentBase base class is required to use lifecycle events
                .AddBaseType(Constants.ComponentBaseClass);

            var orderedMethods = allMethods
                .Select(method => (method, LifecycleManagerService.CheckMethodPageLifecycleHook(method)))
                // Filter out non-lifecycle methods
                .Where(methodTuple => methodTuple.Item2 != null)
                // Order matters within new events so we order before processing
                .OrderBy(methodTuple =>
                {
                    return (int)methodTuple.Item2;
                });

            // Remove old lifecycle methods, sort, and record their content
            foreach (var methodTuple in orderedMethods)
            {
                // This records the statements in the proper collection
                ProcessLifecycleEventMethod(methodTuple.Item1, (WebFormsPageLifecycleEvent)methodTuple.Item2);
                // Refresh node before removing
                var currentMethodNode = currentClassDeclaration.GetCurrentNode(methodTuple.Item1);
                currentClassDeclaration = currentClassDeclaration.RemoveNode(currentMethodNode, SyntaxRemoveOptions.AddElasticMarker);
            }

            // Construct new lifecycle methods and add them to the class
            foreach (var newLifecycleEventKvp in _newLifecycleLines)
            {
                var newLifecycleEvent = newLifecycleEventKvp.Key;
                var newLifecycleEventStatements = newLifecycleEventKvp.Value;

                var newMethodDeclaration = ComponentSyntaxHelper.ConstructComponentLifecycleMethod(newLifecycleEvent, newLifecycleEventStatements);
                currentClassDeclaration = currentClassDeclaration.AddMembers(newMethodDeclaration);
            }

            // If we need to make use of the dispose method, add the IDisposable
            // interface to the class, usings are fine as is because this come from
            // the System namespace
            if (_newLifecycleLines.ContainsKey(BlazorComponentLifecycleEvent.Dispose))
            {
                currentClassDeclaration = currentClassDeclaration.AddBaseType(Constants.DisposableInterface);
            }

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace.ToDisplayString(), currentClassDeclaration);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, CodeSyntaxHelper.BuildUsingStatements(requiredNamespaceNames));

            DoCleanUp();
            LogEnd();

            var result = new[] { new FileInformation(GetNewRelativePath(), Encoding.UTF8.GetBytes(fileText)) };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }

        private string GetNewRelativePath()
        {
            // TODO: Potentially remove certain folders from beginning of relative path
            var newRelativePath = FilePathHelper.AlterFileName(_relativePath,
                oldExtension: Constants.PageCodeBehindExtension,
                newExtension: Constants.RazorCodeBehindFileExtension);

            return Path.Combine(Constants.RazorPageDirectoryName, newRelativePath);
        }

        private void ProcessLifecycleEventMethod(MethodDeclarationSyntax methodDeclaration, WebFormsPageLifecycleEvent lifecycleEvent)
        {
            var statements = (IEnumerable<StatementSyntax>)methodDeclaration.Body.Statements;

            // Dont do anything if the method is empty, no reason to move over nothing
            if (statements.Any())
            {
                statements = statements.AddComment(string.Format(Constants.NewEventRepresentationCommentTemplate, lifecycleEvent.ToString()));

                var blazorLifecycleEvent = LifecycleManagerService.GetEquivalentComponentLifecycleEvent(lifecycleEvent);

                if (_newLifecycleLines.ContainsKey(blazorLifecycleEvent))
                {
                    // Add spacing between last added method
                    statements = statements.Prepend(CodeSyntaxHelper.GetBlankLine());
                    _newLifecycleLines[blazorLifecycleEvent] = _newLifecycleLines[blazorLifecycleEvent].Concat(statements);
                }
                else
                {
                    _newLifecycleLines.Add(blazorLifecycleEvent, statements);
                }
            }
        }
    }
}
