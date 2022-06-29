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

        [SetUp]
        public void SetUp()
        {
            TearDown();
            _configParser = new TagConfigParser(TempTestTagConfigsDir);
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
        public void GetConfigForNode_Generates_Valid_Minimal_Template_Config()
        {
            CopyTestFilesToTempFolder("valid.Minimal.yaml");

            var configMapping = _configParser.GetConfigForNode("valid:Minimal") as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.AreEqual("Namespace.TypeName", configMapping.CodeBehindType);
            Assert.AreEqual("Default", configMapping.CodeBehindHandler);
            Assert.IsEmpty(configMapping.Conditions);
            Assert.IsEmpty(configMapping.Invocations);
            Assert.IsEmpty(configMapping.Templates);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigForNode_Generates_Valid_Minimal_Template_Config_With_Nulled_Out_Values()
        {
            CopyTestFilesToTempFolder("valid.MinimalNulls.yaml");

            var configMapping = _configParser.GetConfigForNode("valid:MinimalNulls") as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.AreEqual("Namespace.TypeName", configMapping.CodeBehindType);
            Assert.AreEqual("Default", configMapping.CodeBehindHandler);
            Assert.IsNull(configMapping.Conditions);
            Assert.IsNull(configMapping.Invocations);
            Assert.IsEmpty(configMapping.Templates);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigForNode_Generates_Valid_Minimal_Template_Config_With_Excluded_Optional_Values()
        {
            CopyTestFilesToTempFolder("valid.MinimalExclusions.yaml");

            var configMapping = _configParser.GetConfigForNode("valid:MinimalExclusions") as TemplateTagConverter;

            Assert.NotNull(configMapping);
            Assert.AreEqual("Namespace.TypeName", configMapping.CodeBehindType);
            Assert.AreEqual("Default", configMapping.CodeBehindHandler);
            Assert.IsNull(configMapping.Conditions);
            Assert.IsNull(configMapping.Invocations);
            Assert.IsEmpty(configMapping.Templates);
        }

        [Test]
        [NonParallelizable]
        public void GetConfigForNode_Fails_To_Recognize_Minimal_Template_Config_With_Extra_Properties()
        {
            CopyTestFilesToTempFolder("invalid.MinimalExtraProperties.yaml");

            Assert.IsNull(_configParser.GetConfigForNode("invalid:MinimalExtraProperties"));
            Assert.True(_configParser.NoConverterTags.Contains("invalid:MinimalExtraProperties"));
        }

        [TestCase("HasAttribute", typeof(HasAttributeTemplateCondition), new[] { "AttributeName" })]
        [TestCase("HasAttributeWithValue", typeof(HasAttributeWithValueTemplateCondition), new[] { "AttributeName", "AttributeValue" })]
        [TestCase("HasParent", typeof(HasParentTemplateCondition), new[] { "ParentTagName" })]
        [TestCase("HasGrandparent", typeof(HasGrandparentTemplateCondition), new[] { "GrandparentTagName" })]
        [NonParallelizable]
        public void GetConfigForNode_Generates_Valid_Template_Config_With_Basic_Condition(string tagName, Type conditionType, IEnumerable<string> expectedProperties)
        {
            CopyTestFilesToTempFolder($"valid.{tagName}.yaml");

            var configMapping = _configParser.GetConfigForNode($"valid:{tagName}") as TemplateTagConverter;

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
        public void GetConfigForNode_Generates_Valid_Template_Config_With_Operator_Condition(string tagName, Type conditionType, int numConditions)
        {
            CopyTestFilesToTempFolder($"valid.{tagName}.yaml");

            var configMapping = _configParser.GetConfigForNode($"valid:{tagName}") as TemplateTagConverter;

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
        public void GetConfigForNode_Generates_Valid_Template_Config_With_Multiple_Conditions()
        {
            CopyTestFilesToTempFolder("valid.MultipleConditions.yaml");

            var configMapping = _configParser.GetConfigForNode("valid:MultipleConditions") as TemplateTagConverter;

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
        public void GetConfigForNode_Fails_To_Recognize_Templates_Containing_Conditions_With_Extra_Properties(string tagName)
        {
            CopyTestFilesToTempFolder($"invalid.{tagName}.yaml");

            Assert.IsNull(_configParser.GetConfigForNode($"invalid:{tagName}"));
            Assert.True(_configParser.NoConverterTags.Contains($"invalid:{tagName}"));
        }


        [TestCase("AddNugetPackage", typeof(AddNugetPackageTemplateInvokable), new[] { "PackageName" })]
        [TestCase("AddUsingDirective", typeof(AddUsingDirectiveTemplateInvokable), new[] { "NamespaceName" })]
        [NonParallelizable]
        public void GetConfigForNode_Generates_Valid_Template_Config_With_Invocation(string tagName, Type invokableType, IEnumerable<string> expectedProperties)
        {
            CopyTestFilesToTempFolder($"valid.{tagName}.yaml");

            var configMapping = _configParser.GetConfigForNode($"valid:{tagName}") as TemplateTagConverter;

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
        public void GetConfigForNode_Generates_Valid_Template_Config_With_Multiple_Invocations()
        {
            CopyTestFilesToTempFolder("valid.MultipleInvocations.yaml");

            var configMapping = _configParser.GetConfigForNode("valid:MultipleInvocations") as TemplateTagConverter;

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
        public void GetConfigForNode_Fails_To_Recognize_Templates_Containing_Invocations_With_Extra_Properties()
        {
            CopyTestFilesToTempFolder($"invalid.AddNugetPackageExtraProperties.yaml");

            Assert.IsNull(_configParser.GetConfigForNode("invalid:AddNugetPackageExtraProperties"));
            Assert.True(_configParser.NoConverterTags.Contains("invalid:AddNugetPackageExtraProperties"));
        }

        [Test]
        [NonParallelizable]
        public void GetConfigForNode_Returns_Null_For_Node_With_No_Config()
        {
            Assert.IsNull(_configParser.GetConfigForNode("Not:RealControl"));
            Assert.True(_configParser.NoConverterTags.Contains("Not:RealControl"));
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
