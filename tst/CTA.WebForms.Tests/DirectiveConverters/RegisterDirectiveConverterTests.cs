using System;
using CTA.WebForms.DirectiveConverters;
using CTA.WebForms.Helpers.ControlHelpers;
using CTA.WebForms.Services;
using NUnit.Framework;

namespace CTA.WebForms.Tests.DirectiveConverters
{
    [TestFixture]
    public class RegisterDirectiveConverterTests
    {
        private const string TestPath = "Default.aspx";
        private const string TestDirectiveName = "Register";
        private const string TestProjectName = "eShopOnBlazor";
        
        private const string TestStandardRegisterDirective =
            @"<%@ Register Src=""~/CustomControls/Counter.ascx"" TagName=""Counter"" TagPrefix=""TCounter"" %>";
        private const string TestDifferentTagName = 
            @"<%@ Register Src=""eShopOnBlazor/Foobar.ascx"" TagName=""Footer"" TagPrefix=""TFooter"" %>";
        private const string TestIncorrectSource =
            @"<%@ Register Src=""Footer.ascx"" TagName=""Footer1"" TagPrefix=""TFooter1"" %>";

        private const string ExpectedStandardRegisterDirective =
            "@using eShopOnBlazor.CustomControls";
        private const string ExpectedDifferentTagName = 
            "@using eShopOnBlazor";
        private readonly string ExpectedIncorrectSource =
            @$"<!-- Cannot convert file name to namespace, file path Footer.ascx does not have a directory -->
<!-- {TestIncorrectSource} -->";
        

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new RegisterDirectiveConverter();
        }

        [Test]
        public void RegisterDirective_Properly_Executes_Standard_Conversion()
        {
            Assert.AreEqual(ExpectedStandardRegisterDirective, _directiveConverter.ConvertDirective(
                TestDirectiveName, TestStandardRegisterDirective, TestPath, TestProjectName, new ViewImportService()));
        }

        [Test]
        public void RegisterDirective_Properly_Converts_Different_TagName()
        {
            Assert.AreEqual(ExpectedDifferentTagName, _directiveConverter.ConvertDirective(
                TestDirectiveName, TestDifferentTagName, TestPath, TestProjectName, new ViewImportService()));
        }

        [Test]
        public void RegisterDirective_Properly_Converts_Invalid_Src()
        {
            Assert.AreEqual(ExpectedIncorrectSource, _directiveConverter.ConvertDirective(
                TestDirectiveName, TestIncorrectSource, TestPath, TestProjectName, new ViewImportService()));
        }
    }
}