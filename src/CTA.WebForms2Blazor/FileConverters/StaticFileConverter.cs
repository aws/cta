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

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            var newPath = Path.Combine("wwwroot", RelativePath);
            var workingDirectory = Environment.CurrentDirectory;
            var projectPrefix = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var fullPath = Path.Combine(projectPrefix, RelativePath);

            FileInformation fi = new FileInformation(newPath, File.ReadAllBytes(fullPath));

            var fileList = new List<FileInformation>();
            fileList.Add(fi);

            return fileList;
        }
    }
}
