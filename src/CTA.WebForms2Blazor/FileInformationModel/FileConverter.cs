using System.Threading.Tasks;
using System.Collections.Generic;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public abstract class FileConverter
    {
        private readonly string _relativePath;

        public string RelativePath { get { return _relativePath; } }

        protected FileConverter(string relativePath)
        {
            _relativePath = relativePath;
        }

        public abstract Task<IEnumerable<FileConverter>> MigrateFileAsync();

        public abstract byte[] GetFileBytes();
    }
}
