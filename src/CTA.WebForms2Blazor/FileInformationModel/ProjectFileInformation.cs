using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public class ProjectFileInformation : FileInformation
    {
        public ProjectFileInformation(string relativePath) : base(relativePath)
        {

        }

        public override byte[] GetFileBytes()
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
