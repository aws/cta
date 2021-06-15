using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public class ViewFileConverter : FileConverter
    {
        public ViewFileConverter(string relativePath) : base(relativePath)
        {

        }

        public override byte[] GetFileBytes()
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<FileConverter>> MigrateFileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
