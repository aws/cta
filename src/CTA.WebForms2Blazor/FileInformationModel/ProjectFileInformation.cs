using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public class ProjectFileInformation : FileInformation
    {
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly WorkspaceManagerService _webFormsWorkspaceManager;

        public ProjectFileInformation(
            string relativePath,
            WorkspaceManagerService blazorWorkspaceManager,
            WorkspaceManagerService webFormsWorkspaceManager
            ) : base(relativePath)
        {
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsWorkspaceManager = webFormsWorkspaceManager;

            _blazorWorkspaceManager.NotifyNewExpectedProject();
            _webFormsWorkspaceManager.NotifyNewExpectedProject();
        }

        public override byte[] GetFileBytes()
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            // TODO: Extract info from project files and
            // call _blazorWorkspaceManager.CreateProjectFile
            // and _webFormsWorkspaceManager.CreateProjectFile

            // TODO: Retrieve cancellation token from thread manager service
            // _blazorWorkspaceManager.WaitUntilAllDocumentsInWorkspace(token);

            // TODO: Extract accumulated project info from 
            // workspace and use it to build the actual
            // project file

            throw new NotImplementedException();
        }
    }
}
