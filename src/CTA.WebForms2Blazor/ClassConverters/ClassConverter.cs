using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public abstract class ClassConverter
    {
        private protected readonly string _relativePath;
        private protected readonly SemanticModel _sourceFileSemanticModel;
        private protected readonly INamedTypeSymbol _originalClassSymbol;

        protected ClassConverter(
            string relativePath,
            SemanticModel sourceFileSemanticModel,
            INamedTypeSymbol originalClassSymbol)
        {
            _relativePath = relativePath;
            _sourceFileSemanticModel = sourceFileSemanticModel;
            _originalClassSymbol = originalClassSymbol;
        }

        public abstract Task<FileInformation> MigrateClassAsync();
    }
}
