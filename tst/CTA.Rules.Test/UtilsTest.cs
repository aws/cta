using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.Rules.Config;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace CTA.Rules.Test
{
    public class UtilsTests : AwsRulesBaseTest
    {
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
            var fileName = "MyFile";
            var extension = "json";
            var mutexName = "mutex";
            var tasks = new List<Task>();
            var fileNames = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => {
                    fileNames.Add(Utils.GenerateUniqueFileName(fileName, extension, mutexName));
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
            var filePath = "test.txt";
            var content = "Test file content";
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => {
                    Utils.ThreadSafeExportStringToFile(filePath, content);
                }));
            }
            var t = Task.WhenAll(tasks);
            t.Wait();

            Assert.True(t.Status == TaskStatus.RanToCompletion);
        }
    }
}