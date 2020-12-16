using System.Collections.Generic;
using CTA.FeatureDetection.Common.Extensions;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Extensions
{
    public class EnumerableExtensionsTests
    {
        [Test]
        public void IsNullOrEmpty_Returns_True_If_Enumerable_Is_Null()
        {
            IEnumerable<int> nullEnumerable = null;

            Assert.True(nullEnumerable.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_Returns_True_If_Enumerable_Is_Empty()
        {
            IEnumerable<int> nullEnumerable = new List<int>();

            Assert.True(nullEnumerable.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_Returns_False_If_Enumerable_Has_Contents()
        {
            IEnumerable<int> nullEnumerable = new [] { 1 };

            Assert.False(nullEnumerable.IsNullOrEmpty());
        }
    }
}