using CTA.WebForms2Blazor.Helpers;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    public class UtilitiesTests
    {
        [Test]
        public void NormalizeNamespaceIdentifier_Prepends_Underscore_For_Invalid_Start()
        {
            var input = "0TestNamespace.SubNamespace";
            var expectedOutput = "_0TestNamespace.SubNamespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public void NormalizeNamespaceIdentifier_Removes_Invalid_Characters()
        {
            var input = "Test$Na!me^sp~ace@2@.##Sub-Namespace##";
            var expectedOutput = "TestNamespace2.SubNamespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public void NormalizeNamespaceIdentifier_Removes_Double_Periods()
        {
            var input = "TestNamespace3..SubNamespace........SubSubNamespace";
            var expectedOutput = "TestNamespace3.SubNamespace.SubSubNamespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public void NormalizeNamespaceIdentifier_Removes_Double_Periods_Created_By_Invalid_Character_Removal()
        {
            var input = "TestNamespace4.$.SubNamespace...@..###..%.SubSubNamespace";
            var expectedOutput = "TestNamespace4.SubNamespace.SubSubNamespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }
    }
}
