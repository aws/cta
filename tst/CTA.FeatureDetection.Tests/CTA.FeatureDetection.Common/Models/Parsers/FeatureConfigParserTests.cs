using System.IO;
using CTA.FeatureDetection.Common.Models.Parsers;
using CTA.FeatureDetection.Tests.Utils;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Models.Parsers
{
    public class FeatureConfigParserTests
    {
        private readonly string _testProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));

        [Test]
        public void Parse_Returns_FeatureConfig_Instance_When_Config_Is_Well_Defined()
        {
            var jsonFilePath = Path.Combine(_testProjectDirectory, "Examples", "Input", "feature_config.json");
            var featureConfigInstance = FeatureConfigParser.Parse(jsonFilePath);

            Assert.NotNull(featureConfigInstance); 
        }

        [Test]
        public void Parse_Throws_Validation_Exception_When_Enum_Value_Is_Invalid()
        {
            var jsonFilePath = Path.Combine(_testProjectDirectory, "Examples", "Input", "test_file_with_nonexistent_feature_scope.json");
            Assert.Throws<Newtonsoft.Json.JsonException>(() => FeatureConfigParser.Parse(jsonFilePath));
        }

        [Test]
        public void Parse_Throws_Validation_Exception_When_Required_Field_Is_Missing()
        {
            var jsonFilePath = Path.Combine(_testProjectDirectory, "Examples", "Input", "test_file_with_missing_feature_scope.json");
            Assert.Throws<Newtonsoft.Json.JsonException>(() => FeatureConfigParser.Parse(jsonFilePath));
        }

        [Test]
        public void Parse_Catches_FileNotFoundException()
        {
            var configFile = "ThisFileDoesNotExist.json";
            Assert.DoesNotThrow(() => FeatureConfigParser.Parse(configFile));
        }
    }
}
