using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public class CodeFileInformation : FileInformation
    {
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly WorkspaceManagerService _webFormsWorkspaceBuilder;

        public CodeFileInformation(
            string relativePath,
            WorkspaceManagerService blazorWorkspaceManager,
            WorkspaceManagerService webFormsWorkspaceManager
            ) : base(relativePath)
        {
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            _webFormsWorkspaceBuilder = webFormsWorkspaceManager;

            _webFormsWorkspaceBuilder.NotifyNewExpectedDocument();

            // TODO: Create class information objects and call
            // _blazorProjectBuilder.NotifyNewExpectedDocument() in
            // each one, this is because potentially multiple files
            // are produced by this file information type
        }

        public override byte[] GetFileBytes()
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            // TODO: Retrieve cancellation token from thread manager service
            // await _webFormsWorkspaceBuilder.WaitUntilAllProjectsInWorkspace(token);

            // TODO: Get project name, and document text
            // _webFormsWorkspaceBuilder.AddDocument("{Project Name}", Path.GetFileName(RelativePath), "{Document Text}");

            // TODO: Call migration functions on class information
            // objects and call _blazorProjectBuilder.AddDocument
            // at the end of the migration process before returning

            throw new NotImplementedException();
        }
    }
}
