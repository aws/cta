using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class StaticResourceFileConverter : FileConverter
    {
        public StaticResourceFileConverter(string sourceProjectPath, string fullPath) : base(sourceProjectPath, fullPath)
        {

        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();

            var newPath = Path.Combine(Constants.WebRootDirectoryName, RelativePath);
            var fullPath = Path.Combine(ProjectPath, RelativePath);

            FileInformation fi = new FileInformation(newPath, File.ReadAllBytes(fullPath));

            var fileList = new List<FileInformation>();
            fileList.Add(fi);

            LogEnd();

            return fileList;
        }
    }
}
