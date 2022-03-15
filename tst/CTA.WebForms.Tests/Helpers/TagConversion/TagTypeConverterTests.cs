using CTA.WebForms.Helpers.TagConversion;
using NUnit.Framework;
using System;

namespace CTA.WebForms.Tests.Helpers.TagConversion
{
    public class TagTypeConverterTests
    {
        private const string SourceAttr = "Source0";
        private const string TargetAttr = "Target0";

        // Would like to use [TestCase(...)]s here but NUnit is having issues running
        // test cases with string arguments over a certain length, so we use a test
        // source here instead
        private static object[] ConvertToType_Source =
        {
            new object[] { "String", "TestString", $"{TargetAttr}=\"TestString\"", "TestString" },
            new object[] { "HtmlBoolean", SourceAttr, TargetAttr, "" },
            new object[] { "HtmlBoolean", "true", TargetAttr, "" },
            new object[] { "HtmlBoolean", "false", "", "" },
            new object[] { "HtmlBoolean", "SomeText", "", "" },
            new object[] { "ComponentBoolean", SourceAttr, $"{TargetAttr}=\"True\"", "True" },
            new object[] { "ComponentBoolean", "true", $"{TargetAttr}=\"True\"", "True" },
            new object[] { "ComponentBoolean", "false", $"{TargetAttr}=\"False\"", "False" },
            new object[] { "ComponentBoolean", "SomeText", $"{TargetAttr}=\"False\"", "False" },
            new object[] { "EventCallback", "EventCallbackMethod", $"{TargetAttr}=\"EventCallbackMethod\"", "EventCallbackMethod" },
            new object[] { "EventHandler", "EventHandlerMethod", $"{TargetAttr}=\"(args) => EventHandlerMethod(null, args)\"", "(args) => EventHandlerMethod(null, args)" }
        };

        [TestCaseSource(nameof(ConvertToType_Source))]
        public void ConvertToType_Properly_Converts_Types(
            string typeName,
            string inputValue,
            string expectedResult,
            string expectedNoTargetResult)
        {
            var result = TagTypeConverter.ConvertToType(SourceAttr, inputValue, TargetAttr, typeName);
            var noTargetResult = TagTypeConverter.ConvertToType(SourceAttr, inputValue, null, typeName);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedNoTargetResult, noTargetResult);
        }

        // Would like to use [TestCase(...)]s here but NUnit is having issues running
        // test cases with string arguments over a certain length, so we use a test
        // source here instead
        private static object[] ConvertToType_With_Bindings_Source =
        {
            new object[] { "<%#: Something0 %>", $"{TargetAttr}=\"@(Something0)\"", "@(Something0)" },
            new object[] { "<%: Something1 %>", $"{TargetAttr}=\"@(Something1)\"", "@(Something1)" },
            new object[] { "<%= Something2 %>", $"{TargetAttr}=\"@(new MarkupString(Something2))\"", "@(new MarkupString(Something2))" },
            new object[] { "S0: <%#: Something0 %> S1: <%: Something1 %>", $"{TargetAttr}=\"S0: @(Something0) S1: @(Something1)\"", "S0: @(Something0) S1: @(Something1)" }
        };

        [TestCaseSource(nameof(ConvertToType_With_Bindings_Source))]
        public void ConvertToType_Properly_Converts_Input_Values_With_Binding_Syntax(
            string inputValue,
            string expectedResult,
            string expectedNoTargetResult)
        {
            var result = TagTypeConverter.ConvertToType(SourceAttr, inputValue, TargetAttr, "String");
            var noTargetResult = TagTypeConverter.ConvertToType(SourceAttr, inputValue, null, "String");

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedNoTargetResult, noTargetResult);
        }

        [Test]
        public void ConvertToType_Throws_Exception_On_Invalid_Type_Argument()
        {
            var exceptionType = typeof(ArgumentException);
            TestDelegate testDelegate = () => TagTypeConverter.ConvertToType(SourceAttr, "Value0", TargetAttr, "InvalidType");

            Assert.Throws(exceptionType, testDelegate);
        }
    }
}
