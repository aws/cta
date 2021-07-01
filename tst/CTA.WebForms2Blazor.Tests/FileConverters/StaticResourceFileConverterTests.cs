using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;
using CTA.WebForms2Blazor.FileInformationModel;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.ControlConverters;

namespace CTA.WebForms2Blazor.Tests.FileConverters
{
    [TestFixture]
    class StaticResourceFileConverterTests
    {
        [Test]
        public async Task TestStaticResourceFileConverter()
        {
            string sourceFilePath = FileConverterSetupFixture.TestStaticResourceFilePath;
            FileConverter fc = new StaticResourceFileConverter(FileConverterSetupFixture.TestProjectPath, sourceFilePath);

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, sourceFilePath);

            Assert.IsTrue(bytes.Length == new FileInfo(sourceFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(Path.Combine("wwwroot", relativePath)));
        }
    }
}

