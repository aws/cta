using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class StaticFileConverter : FileConverter
    {
        public StaticFileConverter(string relativePath) : base(relativePath)
        {

        }

        protected override byte[] GetFileBytes()
        {
            var workingDirectory = Environment.CurrentDirectory;
            var testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var path = Path.Combine(testProjectPath, RelativePath);
            return File.ReadAllBytes(path);
        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            var newPath = Path.Combine("wwwroot", RelativePath);
            FileInformation fi = new StaticFileInformation(newPath, GetFileBytes());

            var fileList = new List<FileInformation>();
            fileList.Add(fi);

            return fileList;
        }
    }
}
