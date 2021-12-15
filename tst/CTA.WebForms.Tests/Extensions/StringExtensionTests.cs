using CTA.WebForms.Extensions;
using NUnit.Framework;

namespace CTA.WebForms.Tests.Extensions
{
    public class StringExtensionTests
    {
        private const string TestText = "Message";

        [Test]
        public void RemoveOuterQuotes_Removes_Single_Quotes()
        {
            Assert.AreEqual(TestText, $"'{TestText}'".RemoveOuterQuotes());
        }

        [Test]
        public void RemoveOuterQuotes_Removes_Double_Quotes()
        {
            Assert.AreEqual(TestText, $"\"{TestText}\"".RemoveOuterQuotes());
        }

        [Test]
        public void RemoveOuterQuotes_Does_Nothing_With_A_Single_Quote()
        {
            var modifiedTestText = "\"";

            Assert.AreEqual(modifiedTestText, modifiedTestText.RemoveOuterQuotes());
        }

        [Test]
        public void RemoveOuterQuotes_Does_Nothing_With_Mismatched_Quotes()
        {
            var modifiedTestText = $"\"{TestText}'";

            Assert.AreEqual(modifiedTestText, modifiedTestText.RemoveOuterQuotes());
        }
    }
}
