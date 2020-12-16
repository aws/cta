using CTA.FeatureDetection.Common;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common
{
    public class UtilsTests
    {
        [Test]
        public void GetCurrentLoggerOrDefault_Does_Not_Return_Null()
        {
            Assert.True(Log.Logger != null);
        }
    }
}