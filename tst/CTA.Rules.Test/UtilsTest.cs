using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.Rules.Config;
using NUnit.Framework;
using System.IO;

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
    }
}