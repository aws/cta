using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public class StaticFileInformation : FileInformation
    {
        public StaticFileInformation(string relativePath) : base(relativePath)
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
