using CTA.WebForms2Blazor.DirectiveConverters;
using NUnit.Framework;
using System;

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
        private static string ExpectedMasterPageFileDirective => $"{Environment.NewLine}@layout TestMasterPage";
        private static string ExpectedInheritsDirective => $"{Environment.NewLine}@inherits TestBaseClass";
        private static string ExpectedTitleDirective => $"{Environment.NewLine}@using Microsoft.AspNetCore.Components.Web.Extensions.Head";
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
            Assert.AreEqual(ExpectedPageDirective, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveName));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_MasterPageFile_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedMasterPageFileDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveMasterPageFileAttribute));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Inherits_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedInheritsDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveInheritsAttribute));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Title_Attribute()
        {
            var expectedText = ExpectedPageDirective + ExpectedTitleDirective + ExpectedTitleTag;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveTitleAttribute));
        }

        [Test]
        public void ConvertDirective_Executes_Multiple_Attribute_Conversions_With_Proper_Ordering()
        {
            var expectedText = ExpectedPageDirective + ExpectedTitleDirective + ExpectedMasterPageFileDirective + ExpectedTitleTag;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirective2Attributes));
        }
    }
}
