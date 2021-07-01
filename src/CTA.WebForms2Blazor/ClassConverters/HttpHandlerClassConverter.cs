﻿using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.IO;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class HttpHandlerClassConverter : ClassConverter
    {
        public HttpHandlerClassConverter(
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
            // NOTE: For now we make no code modifications, just to be
            // ready for the demo and produces files
            // TODO: Modify namespace according to new relative path? Will,
            // need to track a change like that in the reference manager and
            // modify using statements in other files, determing all namespace
            // changes before re-assembling new using statement collection will
            // make this possible
            var sourceClassComponents = GetSourceClassComponents();

            // Http modules are turned into middleware and so we use a new middleware directory
            // TODO: Potentially remove certain folders from beginning of relative path
            return new FileInformation(Path.Combine(Constants.MiddlewareDirectoryName, _relativePath), Encoding.UTF8.GetBytes(sourceClassComponents.FileText));
        }
    }
}