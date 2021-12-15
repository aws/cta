using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.ClassConverters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CTA.WebForms.Extensions;
using System;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using CTA.Rules.Config;

namespace CTA.WebForms.Factories
{
    public class ClassConverterFactory
    {
        private readonly string _sourceProjectPath;
        private LifecycleManagerService _lifecycleManager;
        private TaskManagerService _taskManager;
        private WebFormMetricContext _metricsContext;

        public ClassConverterFactory(string sourceProjectPath,
            LifecycleManagerService lcManager,
            TaskManagerService taskManager,
            WebFormMetricContext metricsContext)
        {
            _sourceProjectPath = sourceProjectPath;
            _lifecycleManager = lcManager;
            _taskManager = taskManager;
            _metricsContext = metricsContext;

            // TODO: Receive services required for ClassConverters
            // via constructor parameters
        }

        public ClassConverter Build(string sourceFileRelativePath, SemanticModel model, TypeDeclarationSyntax typeDeclarationNode)
        {
            try
            {
                // TODO: Add extra handling for non-ClassDeclarationSyntax
                // TypeDeclarationSyntax derived types (interfaces, enums, etc.)

                var symbol = model.GetDeclaredSymbol(typeDeclarationNode);

                if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedGlobalBaseClass))
                    && sourceFileRelativePath.EndsWith(Constants.ExpectedGlobalFileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new GlobalClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _lifecycleManager, _taskManager, _metricsContext);
                }
                // NOTE: The order is important from this point on, mainly because
                // Page-derived classes are also IHttpHandler derived
                if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedPageBaseClass))
                    && sourceFileRelativePath.EndsWith(Constants.PageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new PageCodeBehindClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _taskManager, _metricsContext);
                }

                if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedControlBaseClass))
                    && sourceFileRelativePath.EndsWith(Constants.ControlCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new ControlCodeBehindClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _taskManager, _metricsContext);
                }

                if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedMasterPageBaseClass))
                    && sourceFileRelativePath.EndsWith(Constants.MasterPageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new MasterPageCodeBehindClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _taskManager, _metricsContext);
                }

                if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(Constants.HttpHandlerInterface)))
                {
                    return new HttpHandlerClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _lifecycleManager, _taskManager, _metricsContext);
                }

                if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(Constants.HttpModuleInterface)))
                {
                    return new HttpModuleClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _lifecycleManager, _taskManager, _metricsContext);
                }

                return new UnknownClassConverter(sourceFileRelativePath, _sourceProjectPath, model, typeDeclarationNode, symbol, _taskManager, _metricsContext);
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to build class converter for {sourceFileRelativePath}");
                return null;
            }
        }

        public IEnumerable<ClassConverter> BuildMany(string sourceFileRelativePath, SemanticModel model)
        {
            return model.SyntaxTree.GetNamespaceLevelTypes().Select(node => Build(sourceFileRelativePath, model, node))
                .Where(classConverter => classConverter != null)
                .ToList();
        }
    }
}
