using System;
using CTA.WebForms2Blazor.DirectiveConverters;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.DirectiveConverters
{
    [TestFixture]
    public class ControlDirectiveConverter
    {
        private const string TestPath = "Default.aspx";
        private const string TestDirectiveName = "Control";
        private const string TestProjectName = "eShopOnBlazor";

        private const string TestDirectiveMasterPageFileAttribute = "Page MasterPageFile=\"~/directory/TestMasterPage.Master\"";
        private const string TestDirectiveInheritsAttribute = "Page Inherits=\"TestBaseClass\"";

        private static string ExpectedPageDirective => "@page \"/\"";
        private static string ExpectedMasterPageFileDirective => $"{Environment.NewLine}@layout TestMasterPage";
        private static string ExpectedInheritsDirective => $"{Environment.NewLine}@inherits TestBaseClass";

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new PageDirectiveConverter();
        }

        [Test]
        public void ConvertDirective_Properly_Executes_Directive_General_Conversion()
        {
            Assert.AreEqual(ExpectedPageDirective, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveName, TestPath, TestProjectName, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_MasterPageFile_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedMasterPageFileDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveMasterPageFileAttribute, TestPath, TestProjectName, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Inherits_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedInheritsDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveInheritsAttribute, TestPath, TestProjectName, new ViewImportService()));
        }
    }
}