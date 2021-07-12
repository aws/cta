using CTA.WebForms2Blazor.DirectiveConverters;
using NUnit.Framework;
using System;

namespace CTA.WebForms2Blazor.Tests.DirectiveConverters
{
    public class MasterDirectiveConverterTests
    {
        private const string TestDirectiveName = "Master";

        private const string TestDirectiveMasterPageFileAttribute = "Master MasterPageFile=\"~/directory/TestMasterPage.Master\"";
        private const string TestDirectiveInheritsAttribute = "Master Inherits=\"TestBaseClass\"";
        private const string TestDirective2Attributes = "Master Inherits=\"TestBaseClass\" MasterPageFile=\"~/directory/TestMasterPage.Master\"";
        private const string ExpectedMasterPageFileDirective = "@layout TestMasterPage";
        private const string ExpectedInheritsDirective = "@inherits TestBaseClass";

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new MasterDirectiveConverter();
        }

        [Test]
        public void ConvertDirective_Properly_Executes_Directive_General_Conversion()
        {
            Assert.AreEqual(string.Empty, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveName));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_MasterPageFile_Attribute()
        {
            var expectedText = ExpectedMasterPageFileDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveMasterPageFileAttribute));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Inherits_Attribute()
        {
            var expectedText = ExpectedInheritsDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveInheritsAttribute));
        }

        [Test]
        public void ConvertDirective_Properly_Executes_Multiple_Attribute_Conversions()
        {
            var expectedText = ExpectedInheritsDirective + Environment.NewLine + ExpectedMasterPageFileDirective;

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirective2Attributes));
        }
    }
}
