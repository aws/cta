using CTA.FeatureDetection.Common.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.FeatureDetection.Tests.CTA.FeatureDetection.Common.Models
{
    public class FeatureDetectionResultsTests
    {
        private FeatureDetectionResult FeatureDetectionResult { get; set; }
        private readonly string PresentFeature1 = "PresentFeature1";
        private readonly string PresentFeature2 = "PresentFeature2";
        private readonly string AbsentFeature1 = "AbsentFeature1";
        private readonly string AbsentFeature2 = "AbsentFeature2";

        [SetUp]
        public void SetUp()
        {
            FeatureDetectionResult = new FeatureDetectionResult();
            FeatureDetectionResult.FeatureStatus[PresentFeature1] = true;
            FeatureDetectionResult.FeatureStatus[PresentFeature2] = true;
            FeatureDetectionResult.FeatureStatus[AbsentFeature1] = false;
            FeatureDetectionResult.FeatureStatus[AbsentFeature2] = false;
            FeatureDetectionResult.ProjectPath = @"some\path";
        }

        [Test]
        public void FeatureCohort_Returns_All_Features_In_FeatureStatus()
        {
            var allFeatures = new List<string> { PresentFeature1, PresentFeature2, AbsentFeature1, AbsentFeature2 };
            CollectionAssert.AreEquivalent(allFeatures, FeatureDetectionResult.FeatureCohort);
        }

        [Test]
        public void AbsentFeatures_Returns_All_Features_Set_To_False_In_FeatureStatus()
        {
            var absentFeatures = new List<string> { AbsentFeature1, AbsentFeature2 };
            CollectionAssert.AreEquivalent(absentFeatures, FeatureDetectionResult.AbsentFeatures);
        }

        [Test]
        public void PresentFeatures_Returns_All_Features_Set_To_True_In_FeatureStatus()
        {
            var presentFeatures = new List<string> { PresentFeature1, PresentFeature2 };
            CollectionAssert.AreEquivalent(presentFeatures, FeatureDetectionResult.PresentFeatures);
        }

        [Test]
        public void ProjectPath_Returns_Value_Set_To_ProjectPath()
        {
            Assert.AreEqual(@"some\path", FeatureDetectionResult.ProjectPath);
        }
    }
}
