using System.Threading.Tasks;
using System.Collections.Generic;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public abstract class FileInformation
    {
        private readonly string _relativePath;

        public string RelativePath { get { return _relativePath; } }

        protected FileInformation(string relativePath)
        {
            _relativePath = relativePath;
        }

        public abstract Task<IEnumerable<FileInformation>> MigrateFileAsync();

        public abstract byte[] GetFileBytes();
    }
}
