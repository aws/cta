using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class PageCodeBehindClassConverter : ClassConverter
    {
        public PageCodeBehindClassConverter(
            string relativePath,
            SemanticModel sourceFileSemanticModel,
            INamedTypeSymbol originalClassSymbol)
            : base(relativePath, sourceFileSemanticModel, originalClassSymbol)
        {

        }

        public override async Task MigrateClassAsync()
        {
            throw new NotImplementedException();
        }
    }
}
