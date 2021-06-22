using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ViewFileConverter : FileConverter
    {
        public ViewFileConverter(string sourceProjectPath, string fullPath) : base(sourceProjectPath, fullPath)
        {

        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
