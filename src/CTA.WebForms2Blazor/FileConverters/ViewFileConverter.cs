using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ViewFileConverter : FileConverter
    {
        public ViewFileConverter(string sourceProjectPath, string fullPath) : base(sourceProjectPath, fullPath)
        {
            // TODO: Register file with necessary services
        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            // TODO: Store UI information in necessary services

            // View file converters don't need to return any file
            // information objects, this is to be handled by the
            // code-behinds for each
            return Enumerable.Empty<FileInformation>();
        }
    }
}
