using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis.CSharp;
using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class CodeFileConverter : FileConverter
    {
        private readonly SemanticModel _fileModel;
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly ProjectAnalyzer _webFormsProjectAnaylzer;
        private readonly ClassConverterFactory _classConverterFactory;
        private readonly IEnumerable<ClassConverter> _classConverters;

        public CodeFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ClassConverterFactory classConverterFactory) : base(sourceProjectPath, fullPath)
        {
            // May not need this anymore but not sure yet
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            _webFormsProjectAnaylzer = webFormsProjectAnalyzer;
            _classConverterFactory = classConverterFactory;

            // TODO: Place filename based retrieval in ProjectAnalyzer as method?
            // _fileModel = _webFormsProjectAnaylzer.AnalyzerResult.ProjectBuildResult.SourceFileBuildResults
            //     .Single(r => r.SourceFilePath.EndsWith(Path.GetFileName(RelativePath))).SemanticModel;

            // _classConverters = classConverterFactory.BuildMany(RelativePath, _fileModel);

            // This code is set up to not break unit tests but still work for the demo
            // use code above after demo and just fix unit tests
            _fileModel = _webFormsProjectAnaylzer.AnalyzerResult.ProjectBuildResult?.SourceFileBuildResults?
                .Single(r => r.SourceFilePath.EndsWith(RelativePath))?.SemanticModel;

            if (_fileModel != null)
            {
                _classConverters = classConverterFactory.BuildMany(RelativePath, _fileModel);
            }
        }


        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();

            var classMigrationTasks = _classConverters.Select(classConverter => classConverter.MigrateClassAsync());
            var result = (await Task.WhenAll(classMigrationTasks)).SelectMany(newFileInformation => newFileInformation);

            LogEnd();

            return result;
        }
    }
}
