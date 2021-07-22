using CTA.WebForms2Blazor.DirectiveConverters;
using CTA.WebForms2Blazor.Services;
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
        private const string ExpectedNamespaceDirective = "@namespace Unknown_code_behind_namespace";
        private const string ExpectedMasterPageFileDirective = "@layout TestMasterPage";
        private const string ExpectedInheritsDirective = "<!-- Conversion of Inherits attribute (value: \"TestBaseClass\") for Master directive not currently supported -->";
        private const string ExpectedInheritsLayoutDirective = "@inherits LayoutComponentBase";

        private DirectiveConverter _directiveConverter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directiveConverter = new MasterDirectiveConverter();
        }

        [Test]
        public void ConvertDirective_Properly_Executes_Directive_General_Conversion()
        {
            var expectedText =
$@"{ExpectedNamespaceDirective}
{ExpectedInheritsLayoutDirective}";

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveName, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_MasterPageFile_Attribute()
        {
            var expectedText =
$@"{ExpectedNamespaceDirective}
{ExpectedInheritsLayoutDirective}
{ExpectedMasterPageFileDirective}";

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveMasterPageFileAttribute, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Properly_Converts_Inherits_Attribute()
        {
            var expectedText =
$@"{ExpectedNamespaceDirective}
{ExpectedInheritsLayoutDirective}
{ExpectedInheritsDirective}";

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirectiveInheritsAttribute, new ViewImportService()));
        }

        [Test]
        public void ConvertDirective_Executes_Multiple_Attribute_Conversions_With_Proper_Ordering()
        {
            var expectedText =
$@"{ExpectedNamespaceDirective}
{ExpectedInheritsLayoutDirective}
{ExpectedMasterPageFileDirective}
{ExpectedInheritsDirective}";

            Assert.AreEqual(expectedText, _directiveConverter.ConvertDirective(TestDirectiveName, TestDirective2Attributes, new ViewImportService()));
        }
    }
}
