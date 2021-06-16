using System.Threading.Tasks;
using System.Collections.Generic;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public abstract class FileConverter
    {
        private readonly string _relativePath;

        public string RelativePath { get { return _relativePath; } }

        protected FileConverter(string relativePath)
        {
            _relativePath = relativePath;
        }

        public abstract Task<IEnumerable<FileInformation>> MigrateFileAsync();

    }
}
