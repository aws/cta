using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Factories;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class CodeFileConverter : FileConverter
    {
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly ProjectAnalyzer _webFormsProjectAnaylzer;
        private readonly ClassConverterFactory _classConverterFactory;

        public CodeFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ClassConverterFactory classConverterFactory) : base(sourceProjectPath, fullPath)
        {
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            _webFormsProjectAnaylzer = webFormsProjectAnalyzer;
            _classConverterFactory = classConverterFactory;
            
            // Not sure if the following is needed
            //_webFormsProjectAnaylzer.NotifyNewExpectedDocument();
        }


        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            // TODO: Retrieve cancellation token from thread manager service
            // await _webFormsWorkspaceBuilder.WaitUntilAllProjectsInWorkspace(token);

            // TODO: Get project name, and document text
            // _webFormsWorkspaceBuilder.AddDocument("{Project Name}", Path.GetFileName(RelativePath), "{Document Text}");

            // TODO: Create class information objects and call
            // _blazorProjectBuilder.NotifyNewExpectedDocument() in
            // each one, this is because potentially multiple files
            // are produced by this file information type

            // TODO: Call migration functions on class information
            // objects and call _blazorProjectBuilder.AddDocument
            // at the end of the migration process before returning

            throw new NotImplementedException();
        }
    }
}
