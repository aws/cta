using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.TagConverters;
using CTA.WebForms.TagConverters.TagTemplateConditions;
using CTA.WebForms.TagConverters.TagTemplateInvokables;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CTA.WebForms.Tests.Helpers.TagConversion
{
    [TestFixture]
    public class TagConfigParserTests
    {
        private static string TestTagConfigsDir => Path.Combine(WebFormsTestBase.TestFilesDir, "TestTagConfigs");
        private static string TempTestTagConfigsDir => Path.Combine(WebFormsTestBase.TestFilesDir, "TempTestTagConfigs");

        private TagConfigParser _configParser;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _configParser = new TagConfigParser(TempTestTagConfigsDir);
        }

        [SetUp]
        public void SetUp()
        {
            TearDown();
            Directory.CreateDirectory(TempTestTagConfigsDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TempTestTagConfigsDir))
            {
                Directory.Delete(TempTestTagConfigsDir, true);
            }
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Minimal_Template_Config()
        {
            CopyTestFilesToTempFolder("valid.Minimal.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey("valid:Minimal"));

            var configMapping = configMap["valid:Minimal"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.AreEqual("Namespace.TypeName", configMapping.CodeBehindType);
            Assert.AreEqual("Default", configMapping.CodeBehindHandler);
            Assert.IsEmpty(configMapping.Conditions);
            Assert.IsEmpty(configMapping.Invocations);
            Assert.IsEmpty(configMapping.Templates);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Minimal_Template_Config_With_Nulled_Out_Values()
        {
            CopyTestFilesToTempFolder("valid.MinimalNulls.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey("valid:MinimalNulls"));

            var configMapping = configMap["valid:MinimalNulls"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.AreEqual("Namespace.TypeName", configMapping.CodeBehindType);
            Assert.AreEqual("Default", configMapping.CodeBehindHandler);
            Assert.IsNull(configMapping.Conditions);
            Assert.IsNull(configMapping.Invocations);
            Assert.IsEmpty(configMapping.Templates);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Minimal_Template_Config_With_Excluded_Optional_Values()
        {
            CopyTestFilesToTempFolder("valid.MinimalExclusions.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey("valid:MinimalExclusions"));

            var configMapping = configMap["valid:MinimalExclusions"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.AreEqual("Namespace.TypeName", configMapping.CodeBehindType);
            Assert.AreEqual("Default", configMapping.CodeBehindHandler);
            Assert.IsNull(configMapping.Conditions);
            Assert.IsNull(configMapping.Invocations);
            Assert.IsEmpty(configMapping.Templates);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Fails_To_Recognize_Minimal_Template_Config_With_Extra_Properties()
        {
            CopyTestFilesToTempFolder("invalid.MinimalExtraProperties.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(0, configMap.Keys.Count);
        }

        [TestCase("HasAttribute", typeof(HasAttributeTemplateCondition), new[] { "AttributeName" })]
        [TestCase("HasAttributeWithValue", typeof(HasAttributeWithValueTemplateCondition), new[] { "AttributeName", "AttributeValue" })]
        [TestCase("HasParent", typeof(HasParentTemplateCondition), new[] { "ParentTagName" })]
        [TestCase("HasGrandparent", typeof(HasGrandparentTemplateCondition), new[] { "GrandparentTagName" })]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Template_Config_With_Basic_Condition(string tagName, Type conditionType, IEnumerable<string> expectedProperties)
        {
            CopyTestFilesToTempFolder($"valid.{tagName}.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey($"valid:{tagName}"));

            var configMapping = configMap[$"valid:{tagName}"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.NotNull(configMapping.Conditions);
            Assert.AreEqual(1, configMapping.Conditions.Count());

            var condition = configMapping.Conditions.First();

            Assert.IsInstanceOf(conditionType, condition);

            var typedBasicCondition = Convert.ChangeType(configMapping.Conditions.First(), conditionType);

            foreach (var property in expectedProperties)
            {
                var propValue = conditionType.GetProperty(property).GetValue(typedBasicCondition, null);
                Assert.NotNull(propValue);
            }
        }

        [TestCase("AllConditions", typeof(AllConditionsTemplateCondition), 2)]
        [TestCase("AnyCondition", typeof(AnyConditionTemplateCondition), 3)]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Template_Config_With_Operator_Condition(string tagName, Type conditionType, int numConditions)
        {
            CopyTestFilesToTempFolder($"valid.{tagName}.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey($"valid:{tagName}"));

            var configMapping = configMap[$"valid:{tagName}"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.NotNull(configMapping.Conditions);
            Assert.AreEqual(1, configMapping.Conditions.Count());

            var condition = configMapping.Conditions.Single();

            Assert.IsInstanceOf(conditionType, condition);

            var typedOperatorCondition = Convert.ChangeType(configMapping.Conditions.First(), conditionType);
            var conditionsValue = conditionType.GetProperty("Conditions").GetValue(typedOperatorCondition, null) as IEnumerable<TemplateCondition>;
            
            Assert.NotNull(conditionsValue);
            Assert.AreEqual(numConditions, conditionsValue.Count());
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Template_Config_With_Multiple_Conditions()
        {
            CopyTestFilesToTempFolder("valid.MultipleConditions.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey("valid:MultipleConditions"));

            var configMapping = configMap["valid:MultipleConditions"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.NotNull(configMapping.Conditions);
            Assert.AreEqual(3, configMapping.Conditions.Count());

            var anyCondition = configMapping.Conditions.Where(condition => condition is AnyConditionTemplateCondition)
                .Single() as AnyConditionTemplateCondition;
            var hasGrandparent = configMapping.Conditions.Where(condition => condition is HasGrandparentTemplateCondition)
                .Single() as HasGrandparentTemplateCondition;
            var hasAttribute = configMapping.Conditions.Where(condition => condition is HasAttributeTemplateCondition)
                .Single() as HasAttributeTemplateCondition;

            Assert.NotNull(anyCondition.Conditions);
            Assert.AreEqual(2, anyCondition.Conditions.Count());
            Assert.AreEqual("div", hasGrandparent.GrandparentTagName);
            Assert.AreEqual("Attr3", hasAttribute.AttributeName);
        }

        [TestCase("AnyConditionExtraProperties")]
        [TestCase("HasAttributeExtraProperties")]
        [NonParallelizable]
        public void GetConfigMap_Fails_To_Recognize_Templates_Containing_Conditions_With_Extra_Properties(string tagName)
        {
            CopyTestFilesToTempFolder($"invalid.{tagName}.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(0, configMap.Keys.Count);
        }

        [TestCase("AddNugetPackage", typeof(AddNugetPackageTemplateInvokable), new[] { "PackageName" })]
        [TestCase("AddUsingDirective", typeof(AddUsingDirectiveTemplateInvokable), new[] { "NamespaceName" })]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Template_Config_With_Invocation(string tagName, Type invokableType, IEnumerable<string> expectedProperties)
        {
            CopyTestFilesToTempFolder($"valid.{tagName}.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey($"valid:{tagName}"));

            var configMapping = configMap[$"valid:{tagName}"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.NotNull(configMapping.Invocations);
            Assert.AreEqual(1, configMapping.Invocations.Count());

            var invokable = configMapping.Invocations.First();

            Assert.IsInstanceOf(invokableType, invokable);

            var typedBasicInvokable = Convert.ChangeType(configMapping.Invocations.First(), invokableType);

            foreach (var property in expectedProperties)
            {
                var propValue = invokableType.GetProperty(property).GetValue(typedBasicInvokable, null);
                Assert.NotNull(propValue);
            }
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Valid_Template_Config_With_Multiple_Invocations()
        {
            CopyTestFilesToTempFolder("valid.MultipleInvocations.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(1, configMap.Keys.Count);
            Assert.True(configMap.ContainsKey("valid:MultipleInvocations"));

            var configMapping = configMap["valid:MultipleInvocations"] as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.NotNull(configMapping.Invocations);
            Assert.AreEqual(2, configMapping.Invocations.Count());

            var addNugetPackage = configMapping.Invocations.Where(invocation => invocation is AddNugetPackageTemplateInvokable)
                .Single() as AddNugetPackageTemplateInvokable;
            var addUsingDirective = configMapping.Invocations.Where(invocation => invocation is AddUsingDirectiveTemplateInvokable)
                .Single() as AddUsingDirectiveTemplateInvokable;

            Assert.AreEqual("Some.Other.Package", addNugetPackage.PackageName);
            Assert.AreEqual("X.Y.Z", addUsingDirective.NamespaceName);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Fails_To_Recognize_Templates_Containing_Invocations_With_Extra_Properties()
        {
            CopyTestFilesToTempFolder($"invalid.AddNugetPackageExtraProperties.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(0, configMap.Keys.Count);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Multiple_Valid_Template_Configs()
        {
            CopyTestFilesToTempFolder(
                "valid.Minimal.yaml",
                "valid.MultipleConditions.yaml",
                "valid.AddNugetPackage.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(3, configMap.Keys.Count);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigMap_Generates_Multiple_Valid_Template_Configs_Ignores_Invalid_Configs()
        {
            CopyTestFilesToTempFolder(
                "invalid.AddNugetPackageExtraProperties.yaml",
                "valid.Minimal.yaml",
                "valid.MultipleConditions.yaml",
                "invalid.HasAttributeExtraProperties.yaml",
                "valid.AddNugetPackage.yaml");

            var configMap = _configParser.GetConfigMap();

            Assert.AreEqual(3, configMap.Keys.Count);
        }

        private static void CopyTestFilesToTempFolder(params string[] fileNames)
        {
            // Files in TestTagConfigs directory will automatically be copied to
            // assembly directory, no need to change individual file properties

            foreach (var fileName in fileNames)
            {
                var sourceFilePath = Path.Combine(TestTagConfigsDir, fileName);
                var targetFilePath = Path.Combine(TempTestTagConfigsDir, fileName);

                File.Copy(sourceFilePath, targetFilePath);
            }
        }
    }
}
