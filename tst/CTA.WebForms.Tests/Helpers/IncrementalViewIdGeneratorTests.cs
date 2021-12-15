using CTA.WebForms.Helpers;
using NUnit.Framework;

namespace CTA.WebForms.Tests.Helpers
{
    public class IncrementalViewIdGeneratorTests
    {
        [Test]
        public void GetNewGeneratedId_Returns_Properly_Formatted_Id()
        {
            var nextIdNumber = IncrementalViewIdGenerator.NextGeneratedIdNumber;

            Assert.AreEqual($"GeneratedId{nextIdNumber}", IncrementalViewIdGenerator.GetNewGeneratedId());
        }

        [Test]
        public void GetNewGeneratedId_Increments_After_Call()
        {
            var firstIdNumber = IncrementalViewIdGenerator.NextGeneratedIdNumber;
            IncrementalViewIdGenerator.GetNewGeneratedId();
            var nextIdNumber = IncrementalViewIdGenerator.NextGeneratedIdNumber;

            Assert.AreEqual(firstIdNumber + 1, nextIdNumber);
        }
    }
}
