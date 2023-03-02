using System;
using CTA.WebForms.DirectiveConverters;
using CTA.WebForms.Services;
using NUnit.Framework;

namespace CTA.WebForms.Tests.DirectiveConverters
{
    [TestFixture]
    public class ControlDirectiveConverterTests
    {
        private const string TestPath = "Default.aspx";
        private const string TestDirectiveName = "Control";
        private const string TestProjectName = "eShopOnBlazor";
        
        private const string TestStandardControlDirective =
            @"<%@ Control Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Counter.ascx.cs"" Inherits=""eShopLegacyWebForms.Counter"" %>";

        private const string ExpectedStandardControlDirective =
            @"<!-- Conversion of AutoEventWireup attribute (value: ""true"") for Control directive not currently supported -->";

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new ControlDirectiveConverter();
        }

        [Test]
        public void RegisterDirective_Properly_Executes_Standard_Conversion()
        {
            Assert.AreEqual(
                ExpectedStandardControlDirective, 
                _directiveConverter.ConvertDirective(
                    TestDirectiveName, 
                    TestStandardControlDirective, 
                    TestPath, 
                    TestProjectName, 
                    new ViewImportService()));
        }
    }
}