using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.FileConverters
{
    [TestFixture]
    class StaticResourceFileConverterTests
    {
        [Test]
        public async Task TestStaticResourceFileConverter()
        {
            string sourceFilePath = FileConverterSetupFixture.TestStaticResourceFilePath;
            FileConverter fc = new StaticResourceFileConverter(FileConverterSetupFixture.TestProjectPath, sourceFilePath, new HostPageService(), new TaskManagerService());

            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            byte[] bytes = fi.FileBytes;

            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, sourceFilePath);

            Assert.IsTrue(bytes.Length == new FileInfo(sourceFilePath).Length);
            Assert.IsTrue(fi.RelativePath.Equals(Path.Combine("wwwroot", relativePath)));
        }
    }
}

