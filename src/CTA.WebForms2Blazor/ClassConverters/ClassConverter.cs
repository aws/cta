using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
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

        protected ClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax, 
            INamedTypeSymbol originalClassSymbol)
        {
            _relativePath = relativePath;
            _sourceProjectPath = sourceProjectPath;
            _fullPath = Path.Combine(sourceProjectPath, relativePath);
            _sourceFileSemanticModel = sourceFileSemanticModel;
            _originalDeclarationSyntax = originalDeclarationSyntax;
            _originalClassSymbol = originalClassSymbol;
        }

        public abstract Task<FileInformation> MigrateClassAsync();
    }
}
