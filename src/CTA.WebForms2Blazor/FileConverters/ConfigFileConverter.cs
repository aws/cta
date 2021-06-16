using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ConfigFileConverter : FileConverter
    {
        public ConfigFileConverter(string relativePath) : base(relativePath)
        {

        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
