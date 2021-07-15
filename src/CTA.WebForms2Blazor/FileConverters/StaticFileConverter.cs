using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class StaticFileConverter : FileConverter
    {
        public StaticFileConverter(string sourceProjectPath, string fullPath) : base(sourceProjectPath, fullPath)
        {

        }
        
        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();

            FileInformation fi = new FileInformation(RelativePath, File.ReadAllBytes(FullPath));

            var fileList = new List<FileInformation>();
            fileList.Add(fi);

            LogEnd();

            return Task.FromResult((IEnumerable<FileInformation>)fileList);
        }
    }
}
