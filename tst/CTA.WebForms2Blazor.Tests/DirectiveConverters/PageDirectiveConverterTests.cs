using CTA.WebForms2Blazor.DirectiveConverters;
using CTA.WebForms2Blazor.Services;
using NUnit.Framework;
using System;
using System.Text;

namespace CTA.WebForms2Blazor.Tests.DirectiveConverters
{
    [TestFixture]
    public class PageDirectiveConverterTests
    {
        private const string TestDirectiveName = "Page";

        private const string TestDirectiveMasterPageFileAttribute = "Page MasterPageFile=\"~/directory/TestMasterPage.Master\"";
        private const string TestDirectiveInheritsAttribute = "Page Inherits=\"TestBaseClass\"";
        private const string TestDirectiveTitleAttribute = "Page Title=\"TestTitle\"";
        private const string TestDirective2Attributes = "Page Title=\"TestTitle\" MasterPageFile=\"~/directory/TestMasterPage.Master\"";

        private static string ExpectedPageDirective => "@page \"Unkown_page_route\"";
        private static string ExpectedTitleDirective => "@using Microsoft.AspNetCore.Components.Web.Extensions.Head";
        private static string ExpectedMasterPageFileDirective => $"{Environment.NewLine}@layout TestMasterPage";
        private static string ExpectedInheritsDirective => $"{Environment.NewLine}@inherits TestBaseClass";
        private static string ExpectedTitleTag => $"{Environment.NewLine}<Title value=\"TestTitle\" />";

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new PageDirectiveConverter();
        }

        [Test]
        public void ConvertDirective_Properly_Executes_Directive_General_Conversion()
        {
            Assert.AreEqual(ExpectedPageDirective, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveName, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_MasterPageFile_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedMasterPageFileDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveMasterPageFileAttribute, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Inherits_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedInheritsDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveInheritsAttribute, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Title_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedTitleTag;
            var viewImportService = new ViewImportService();

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveTitleAttribute, viewImportService));

            var importsText = Encoding.UTF8.GetString(viewImportService.ConstructImportsFile().FileBytes);

            Assert.True(importsText.Contains(ExpectedTitleDirective));
        }

        [Test]
        public void ConvertDirective_Executes_Multiple_Attribute_Conversions_With_Proper_Ordering()
        {
            var expectedText = ExpectedPageDirective + ExpectedMasterPageFileDirective + ExpectedTitleTag;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirective2Attributes, new ViewImportService()));
        }
    }
}
