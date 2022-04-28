using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.Rules.Config;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTA.Rules.Common.Helpers;

namespace CTA.Rules.Test
{
    [TestFixture]
    public class UtilsTests : AwsRulesBaseTest
    {
        private const string TempDir = nameof(UtilsTests);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!Directory.Exists(TempDir))
            {
                Directory.CreateDirectory(TempDir);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
        }

        [SetUp]
        public void Setup()
        {
            Setup(GetType());
        }

        [Test]
        public void Parse_Throws_Validation_Exception_When_Enum_Value_Is_Invalid()
        {
            var jsonFilePath = GetTstPath(Path.Combine("CTA.FeatureDetection.Tests", "Examples", "Templates", "test_file_with_nonexistent_feature_scope.json"));
            var jsonContent = File.ReadAllText(jsonFilePath);

            Assert.Throws<Newtonsoft.Json.JsonException>(() => Utils.ValidateJsonObject(jsonContent, typeof(FeatureConfig)));
        }

        [Test]
        public void Parse_Throws_Validation_Exception_When_Required_Field_Is_Missing()
        {
            var jsonFilePath = GetTstPath(Path.Combine("CTA.FeatureDetection.Tests", "Examples", "Templates", "test_file_with_missing_feature_scope.json"));
            var jsonContent = File.ReadAllText(jsonFilePath);

            Assert.Throws<Newtonsoft.Json.JsonException>(() => Utils.ValidateJsonObject(jsonContent, typeof(FeatureConfig)));
        }

        [Test]
        public void ConvertToSHA256Hex_Encrypts_String_To_Expected_Hash_Value()
        {
            var input = "Hello World!";
            var hash = EncryptionHelper.ConvertToSHA256Hex(input);
            var expectedHash = "7f83b1657ff1fc53b92dc18148a1d65dfc2d4b1fa3d677284addd200126d9069";

            Assert.AreEqual(expectedHash, hash);
        }

        [Test]
        public void GenerateUniqueFileName_Maintains_Uniqueness_Across_Concurrent_Processes()
        {
            var filePath = "MyFile.json";
            var mutexName = "mutex";
            var tasks = new List<Task>();
            var fileNames = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => {
                    fileNames.Add(Utils.GenerateUniqueFileName(filePath, mutexName));
                }));
            }
            var t = Task.WhenAll(tasks);
            t.Wait();

            Assert.True(t.Status == TaskStatus.RanToCompletion);
            CollectionAssert.AllItemsAreUnique(fileNames);
        }

        [Test]
        public void ThreadSafeExportStringToFile_Permits_Concurrent_Processes()
        {
            var filePath = Path.Combine(TempDir, "test.txt");
            var content = "Test file content";
            var tasks = new List<Task>();
            var filePaths = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => {
                    var writtenFilePath = Utils.ThreadSafeExportStringToFile(filePath, content);
                    filePaths.Add(writtenFilePath);
                }));
            }
            var t = Task.WhenAll(tasks);
            t.Wait();

            Assert.True(t.Status == TaskStatus.RanToCompletion);

            // Since concurrent file access is permitted, there should be no naming conflicts,
            // and therefore no files should be renamed
            Assert.True(filePaths.All(f => f.Equals(filePath)));

            // Confirm that text was written to file
            Assert.AreEqual(content, File.ReadAllText(filePath));
        }

        [Test]
        public void Test_Is_VisualBasic_Project()
        {
            Assert.IsFalse(VisualBasicUtils.IsVisualBasicProject("test.csproj"));
            Assert.IsTrue(VisualBasicUtils.IsVisualBasicProject("C://user/john/repos/test.vbproj"));
        }
    }
}