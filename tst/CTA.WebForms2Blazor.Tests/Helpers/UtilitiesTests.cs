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
            var input = "Test$Na!me^sp~ace@2@.##Sub*Namespace##";
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

        [Test]
        public void NormalizeNamespaceIdentifier_Replaces_Hyphens_With_Underscores()
        {
            var input = "TestNamespace-5.Sub--Namespace.Sub--Sub--Namespace";
            var expectedOutput = "TestNamespace_5.Sub__Namespace.Sub__Sub__Namespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public void NormalizeNamespaceIdentifier_Replaces_Spaces_With_Underscores()
        {
            var input = "TestNamespace 6.Sub  Namespace.Sub  Sub  Namespace";
            var expectedOutput = "TestNamespace_6.Sub__Namespace.Sub__Sub__Namespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public void NormalizeNamespaceIdentifier_Replaces_Mixed_Hyphens_And_Spaces_With_Underscores()
        {
            var input = "TestNamespace-7.Sub  Namespace.Sub- Sub -Namespace";
            var expectedOutput = "TestNamespace_7.Sub__Namespace.Sub__Sub__Namespace";
            var output = Utilities.NormalizeNamespaceIdentifier(input);

            Assert.AreEqual(expectedOutput, output);
        }
    }
}
