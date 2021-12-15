using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.WebForms2Blazor.Extensions;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public abstract class ClassConverter
    {
        private protected readonly string _relativePath;
        private protected readonly string _sourceProjectPath;
        private protected readonly string _fullPath;
        private protected readonly SemanticModel _sourceFileSemanticModel;
        private protected readonly TypeDeclarationSyntax _originalDeclarationSyntax;
        private protected readonly INamedTypeSymbol _originalClassSymbol;
        private protected readonly TaskManagerService _taskManager;
        private protected int _taskId;

        public string FullPath { get { return _fullPath; } }
        public string OriginalClassName { get { return _originalDeclarationSyntax.Identifier.ToString(); } }

        protected ClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager)
        {
            _relativePath = relativePath;
            _sourceProjectPath = sourceProjectPath;
            _fullPath = Path.Combine(sourceProjectPath, relativePath);
            _sourceFileSemanticModel = sourceFileSemanticModel;
            _originalDeclarationSyntax = originalDeclarationSyntax;
            _originalClassSymbol = originalClassSymbol;
            _taskManager = taskManager;

            // We want to force the use of the task manager even if each class doesn't
            // necessarily have to do any managed runs, this is because they may end up
            // unblocking other processes by simply running normally
            _taskId = _taskManager.RegisterNewTask();
            LogHelper.LogInformation(string.Format(
                Constants.RegisteredAsTaskLogTemplate,
                GetType().Name,
                OriginalClassName,
                _fullPath,
                _taskId));
        }

        public abstract Task<IEnumerable<FileInformation>> MigrateClassAsync();

        private protected SourceClassComponents GetSourceClassComponents()
        {
            var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            var usingStatements = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces);
            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace?.ToDisplayString(), _originalDeclarationSyntax);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, usingStatements);

            return new SourceClassComponents(requiredNamespaces, usingStatements, namespaceNode, fileText);
        }

        private protected void DoCleanUp()
        {
            // TODO: Put other general clean up
            // tasks here as they come up
            _taskManager.RetireTask(_taskId);
        }

        private protected void LogStart()
        {
            LogHelper.LogInformation(string.Format(
                Constants.StartedForAtLogTemplate,
                GetType().Name,
                Constants.ClassMigrationLogAction,
                OriginalClassName,
                _fullPath));
        }

        private protected void LogEnd()
        {
            LogHelper.LogInformation(string.Format(
                Constants.EndedForAtLogTemplate,
                GetType().Name,
                Constants.ClassMigrationLogAction,
                OriginalClassName,
                _fullPath));
        }

        private protected class SourceClassComponents
        {
            public IEnumerable<string> RequiredNamespaces { get; }
            public IEnumerable<UsingDirectiveSyntax> UsingStatements { get; }
            public NamespaceDeclarationSyntax NamespaceNode { get; }
            public string FileText { get; }

            public SourceClassComponents(
                IEnumerable<string> requiredNamespaces,
                IEnumerable<UsingDirectiveSyntax> usingStatements,
                NamespaceDeclarationSyntax namespaceNode,
                string fileText)
            {
                RequiredNamespaces = requiredNamespaces;
                UsingStatements = usingStatements;
                NamespaceNode = namespaceNode;
                FileText = fileText;
            }

            public override string ToString()
            {
                // Just added ToString override for
                // convenience but we can just use
                // FileText property which is more
                // descriptive
                return FileText;
            }
        }
    }
}
