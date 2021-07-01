using System.Collections.Generic;
using System.Linq;
using CTA.WebForms2Blazor.ClassConverters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CTA.WebForms2Blazor.Extensions;
using System;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.Factories
{
    public class ClassConverterFactory
    {
        private readonly string _sourceProjectPath;
        private LifecycleManagerService _lifecycleManager;
        private TaskManagerService _taskManager;

        public ClassConverterFactory(string sourceProjectPath,
            LifecycleManagerService lcManager,
            TaskManagerService taskManager)
        {
            _sourceProjectPath = sourceProjectPath;
            _lifecycleManager = lcManager;
            _taskManager = taskManager;

            // TODO: Receive services required for ClassConverters
            // via constructor parameters
        }

        public ClassConverter Build(string sourceFileRelativePath, SemanticModel model, TypeDeclarationSyntax typeDeclarationNode)
        {
            // TODO: Add extra handling for non-ClassDeclarationSyntax
            // TypeDeclarationSyntax derived types (interfaces, enums, etc.)

            var symbol = model.GetDeclaredSymbol(typeDeclarationNode);

            if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedGlobalBaseClass))
                && sourceFileRelativePath.EndsWith(Constants.ExpectedGlobalFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                return new GlobalClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _lifecycleManager, _taskManager);
            }
            // NOTE: The order is important from this point on, mainly because
            // Page-derived classes are also IHttpHandler derived
            else if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedPageBaseClass))
                && sourceFileRelativePath.EndsWith(Constants.PageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return new PageCodeBehindClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol);
            }
            else if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedControlBaseClass))
                && sourceFileRelativePath.EndsWith(Constants.ControlCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return new ControlCodeBehindClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol);
            }
            else if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedMasterPageBaseClass))
                && sourceFileRelativePath.EndsWith(Constants.MasterPageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return new MasterPageCodeBehindClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol);
            }
            else if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(Constants.HttpHandlerInterface)))
            {
                return new HttpHandlerClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol);
            }
            else if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(Constants.HttpModuleInterface)))
            {
                return new HttpModuleClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol);
            }
            else
            {
                return new UnknownClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol);
            }
        }

        public IEnumerable<ClassConverter> BuildMany(string sourceFileRelativePath, SemanticModel model)
        {
            return model.SyntaxTree.GetNamespaceLevelTypes().Select(node => Build(sourceFileRelativePath, model, node));
        }
    }
}
