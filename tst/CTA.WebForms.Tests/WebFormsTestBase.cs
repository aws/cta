using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace CTA.WebForms.Tests
{
    [TestFixture]
    public class WebFormsTestBase
    {
        public static string AssemblyDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string TestingAreaDir => Path.Combine(AssemblyDir, "TestingArea");
        public static string TestFilesDir => Path.Combine(TestingAreaDir, "TestFiles");

        [OneTimeSetUp]
        public void BaseOneTimeSetup()
        {
            CopyTestTagConfigs();
        }

        private void CopyTestTagConfigs()
        {
            // Project configured to copy TempTagConfigs folder to output directory
            // so no extra action necessary here
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var tempTemplatesDir = Path.Combine(assemblyDir, "TempTagConfigs");
            if (Directory.Exists(tempTemplatesDir))
            {
                var files = Directory.EnumerateFiles(tempTemplatesDir, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(tempTemplatesDir, file);
                    var targetFile = Path.Combine(Rules.Config.Constants.TagConfigsExtractedPath, relativePath);
                    var targetFileDir = Path.GetDirectoryName(targetFile);

                    Directory.CreateDirectory(targetFileDir);
                    File.Copy(file, targetFile, true);
                }
            }
        }
    }
}
