using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class GlobalClassConverter : ClassConverter
    {
        public GlobalClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol)
        {
            // TODO: Register with the necessary services
        }

        public override async Task<FileInformation> MigrateClassAsync()
        {
            // TODO: Modify namespace according to new relative path? Will,
            // need to track a change like that in the reference manager and
            // modify using statements in other files, determing all namespace
            // changes before re-assembling new using statement collection will
            // make this possible
            var sourceClassComponents = GetSourceClassComponents();

            // Global.asax.cs turns into Startup.cs
            var newRelativePath = Path.Combine(Path.GetDirectoryName(_relativePath), Constants.StartupFileName);

            // Get type features
            var descendantNodes = _originalDeclarationSyntax.DescendantNodes();
            var fields = descendantNodes.OfType<FieldDeclarationSyntax>();
            var properties = descendantNodes.OfType<PropertyDeclarationSyntax>();
            var methods = descendantNodes.OfType<MethodDeclarationSyntax>();

            // TODO: Check session prefix
            var sessionLifecycleMethods = methods.Where(method => false);

            // TODO: Check application prefix + event name
            var applicationLifecycleMethods = methods.Where(method => false);
            // TODO: Register with application lifecycle manager

            var keepableMethods = methods.Where(method =>
                !sessionLifecycleMethods.Contains(method) &&
                !applicationLifecycleMethods.Contains(method));

            // TODO: Add commented out session lifecycle methods after
            // final keepable method

            // TODO: Get ordered registrations for lifecycle manager

            // TODO: Get usings required by middleware and maybe services
            // then merge them into existing usings list
            var requiredNamespaces = _sourceFileSemanticModel.GetNamespacesReferencedByType(_originalDeclarationSyntax);
            var usings = CodeSyntaxHelper.BuildUsingStatements(requiredNamespaces.Select(namespaceSymbol => namespaceSymbol.ToDisplayString()));

            // TODO: Add some kind of comment about services somehow and
            // add registration statements
            var startupClassDeclaration = StartupSyntaxHelper.BuildStartupClass(
                additionalFieldDeclarations: fields,
                additionalPropertyDeclarations: properties,
                additionalMethodDeclarations: methods);

            var containingNamespace = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace.Name, startupClassDeclaration);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(containingNamespace, usings);
            
            return new FileInformation(newRelativePath, Encoding.UTF8.GetBytes(fileText));
        }
    }
}
