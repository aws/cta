using System.Collections.Generic;
using System.Linq;
using CTA.WebForms2Blazor.ClassConverters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CTA.WebForms2Blazor.Extensions;
using System;

namespace CTA.WebForms2Blazor.Factories
{
    public class ClassConverterFactory
    {
        private const string ExpectedGlobalBaseClass = "HttpApplication";
        private const string ExpectedPageBaseClass = "Page";
        private const string ExpectedControlBaseClass = "UserControl";
        private const string ExpectedMasterPageBaseClass = "MasterPage";
        private const string ExpectedGlobalFileName = "Global.asax.cs";
        private const string HttpHandlerInterface = "IHttpHandler";
        private const string HttpModuleInterface = "IHttpModule";
        private const string PageCodeBehindExtension = ".aspx.cs";
        private const string ControlCodeBehindExtension = ".ascx.cs";
        private const string MasterPageCodeBehindExtension = ".Master.cs";

        public ClassConverterFactory()
        {
            // TODO: Receive services required for ClassConverters
            // via constructor parameters
        }

        public ClassConverter Build(string sourceFileRelativePath, SemanticModel model, TypeDeclarationSyntax typeDeclarationNode)
        {
            // TODO: Add extra handling for non-ClassDeclarationSyntax
            // TypeDeclarationSyntax derived types (interfaces, enums, etc.)

            var symbol = model.GetDeclaredSymbol(typeDeclarationNode);

            if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(ExpectedGlobalBaseClass))
                && sourceFileRelativePath.EndsWith(ExpectedGlobalFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                return new GlobalClassConverter(sourceFileRelativePath, model, symbol);
            }
            // NOTE: The order is important from this point on, mainly because
            // Page-derived classes are also IHttpHandler derived
            else if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(ExpectedPageBaseClass))
                && sourceFileRelativePath.EndsWith(PageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return new PageCodeBehindClassConverter(sourceFileRelativePath, model, symbol);
            }
            else if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(ExpectedControlBaseClass))
                && sourceFileRelativePath.EndsWith(ControlCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return new ControlCodeBehindClassConverter(sourceFileRelativePath, model, symbol);
            }
            else if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(ExpectedMasterPageBaseClass))
                && sourceFileRelativePath.EndsWith(MasterPageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return new MasterPageCodeBehindClassConverter(sourceFileRelativePath, model, symbol);
            }
            else if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(HttpHandlerInterface)))
            {
                return new HttpHandlerClassConverter(sourceFileRelativePath, model, symbol);
            }
            else if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(HttpModuleInterface)))
            {
                return new HttpModuleClassConverter(sourceFileRelativePath, model, symbol);
            }
            else
            {
                return new UnknownClassConverter(sourceFileRelativePath, model, symbol);
            }
        }

        public IEnumerable<ClassConverter> BuildMany(string sourceFileRelativePath, SemanticModel model)
        {
            return model.SyntaxTree.GetNamespaceLevelTypes().Select(node => Build(sourceFileRelativePath, model, node));
        }
    }
}
