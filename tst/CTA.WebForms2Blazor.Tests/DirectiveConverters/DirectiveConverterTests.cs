using CTA.WebForms2Blazor.DirectiveConverters;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor.Tests.DirectiveConverters
{
    public class DirectiveConverterTests
    {
        private const string TestPath = "UControl.ascx";
        private const string TestDirectiveName = "DirectiveName";
        private const string TestProjectName = "eShopOnBlazor";

        private const string TestDirective1Attribute = "DirectiveName Attr1=\"value1\"";
        private const string TestDirective2Attributes = "DirectiveName Attr1=\"value1\" Attr2=\"value2\"";

        private static string ExpectedUnknownDirectiveComment => $"<!-- General conversion for DirectiveName directive not currently supported -->";
        private static string ExpectedUnknownAttributeComment1 => $"{Environment.NewLine}<!-- Conversion of Attr1 attribute (value: \"value1\") for DirectiveName directive not currently supported -->";
        private static string ExpectedUnknownAttributeComment2 => $"{Environment.NewLine}<!-- Conversion of Attr2 attribute (value: \"value2\") for DirectiveName directive not currently supported -->";

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new DirectiveConverter();
        }

        [Test]
        public void ConvertDirective_Properly_Constructs_General_Directive_Unknown_Comment()
        {
            Assert.AreEqual(ExpectedUnknownDirectiveComment, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveName, TestPath, TestProjectName, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Constructs_Single_Attribute_Unknown_Comment()
        {
            var expectedText = ExpectedUnknownDirectiveComment + ExpectedUnknownAttributeComment1;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirective1Attribute, TestPath, TestProjectName, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Constructs_Multiple_Attribute_Unknown_Comments()
        {
            var expectedText = ExpectedUnknownDirectiveComment + ExpectedUnknownAttributeComment1 + ExpectedUnknownAttributeComment2;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirective2Attributes, TestPath, TestProjectName, new ViewImportService()));
        }
    }
}
