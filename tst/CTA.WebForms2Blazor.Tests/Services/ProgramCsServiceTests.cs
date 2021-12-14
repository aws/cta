using CTA.WebForms2Blazor.Services;
using NUnit.Framework;
using System.Text;

namespace CTA.WebForms2Blazor.Tests.Services
{
    public class ProgramCsServiceTests
    {
        private const string TestNamespace = "TestNamespace";
        private const string ExpectedPath = "Program.cs";
        private const string ExpectedContent =
@"using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TestNamespace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}";

        private ProgramCsService _programCsService;

        [SetUp]
        public void SetUp()
        {
            _programCsService = new ProgramCsService();
            _programCsService.ProgramCsNamespace = TestNamespace;
        }

        [Test]
        public void ConstructProgramCsFile_Properly_Creates_File_Contents()
        {
            var fileBytes = _programCsService.ConstructProgramCsFile().FileBytes;
            var actualContent = Encoding.UTF8.GetString(fileBytes);

            Assert.AreEqual(ExpectedContent, actualContent);
        }

        [Test]
        public void ConstructProgramCsFile_Writes_To_Correct_Path()
        {
            var actualPath = _programCsService.ConstructProgramCsFile().RelativePath;

            Assert.AreEqual(ExpectedPath, actualPath);
        }
    }
}
