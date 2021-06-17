using System;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class MasterPageCodeBehindClassConverter : ClassConverter
    {
        public MasterPageCodeBehindClassConverter(
            string relativePath,
            SemanticModel sourceFileSemanticModel,
            INamedTypeSymbol originalClassSymbol)
            : base(relativePath, sourceFileSemanticModel, originalClassSymbol)
        {

        }

        public override async Task<FileInformation> MigrateClassAsync()
        {
            throw new NotImplementedException();
        }
    }
}
