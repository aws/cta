using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class CodeFileConverter : FileConverter
    {
        private readonly SemanticModel _fileModel;
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly IEnumerable<ClassConverter> _classConverters;

        public CodeFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ClassConverterFactory classConverterFactory) : base(sourceProjectPath, fullPath)
        {
            // May not need this anymore but not sure yet
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            // TODO: Retrieve the semantic model for the document from codelyzer results and uncomment line below
            // _classConverters = classConverterFactory.BuildMany(RelativePath, _fileModel);
        }


        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            var classMigrationTasks = _classConverters.Select(classConverter => classConverter.MigrateClassAsync());

            return await Task.WhenAll(classMigrationTasks);
        }
    }
}
