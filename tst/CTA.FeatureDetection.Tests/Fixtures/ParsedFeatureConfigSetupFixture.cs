using System.IO;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Parsers;
using CTA.FeatureDetection.Tests.Utils;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests
{
    [SetUpFixture]
    public class ParsedFeatureConfigSetupFixture
    {
        public static FeatureConfig WellDefinedFeatureConfig { get; private set; }
        public static FeatureConfig FeatureConfigWithNonexistentAssembly { get; private set; }
        public static FeatureConfig FeatureConfigWithNonexistentFeature { get; private set; }
        public static FeatureConfig FeatureConfigWithNonexistentFeatureProperty { get; private set; }
        public static FeatureConfig FeatureConfigWithNonexistentNamespace { get; private set; }
        public static FeatureConfig FeatureConfigWithDuplicateFeatures { get; private set; }
        public static FeatureConfig FeatureConfigWithInvalidFeature { get; private set; }

        public static readonly string TestProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));

        [OneTimeSetUp]
        public void Setup()
        {
            var jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "feature_config.json");
            WellDefinedFeatureConfig = FeatureConfigParser.Parse(jsonFilePath);

            jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "test_file_with_nonexistent_assembly_path.json");
            FeatureConfigWithNonexistentAssembly = FeatureConfigParser.Parse(jsonFilePath);

            jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "test_file_with_nonexistent_feature.json");
            FeatureConfigWithNonexistentFeature = FeatureConfigParser.Parse(jsonFilePath);

            jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "test_file_with_nonexistent_feature_property.json");
            FeatureConfigWithNonexistentFeatureProperty = FeatureConfigParser.Parse(jsonFilePath);

            jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "test_file_with_nonexistent_namespace.json");
            FeatureConfigWithNonexistentNamespace = FeatureConfigParser.Parse(jsonFilePath);

            jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "test_file_with_duplicate_features.json");
            FeatureConfigWithDuplicateFeatures = FeatureConfigParser.Parse(jsonFilePath);

            jsonFilePath = Path.Combine(TestProjectDirectory, "Examples", "Input", "test_file_with_invalid_feature.json");
            FeatureConfigWithInvalidFeature = FeatureConfigParser.Parse(jsonFilePath);
        }
    }
}
