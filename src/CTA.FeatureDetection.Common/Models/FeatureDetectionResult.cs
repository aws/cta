using System.Collections.Generic;
using System.Linq;

namespace CTA.FeatureDetection.Common.Models
{
    /// <summary>
    /// Contains resulting information from an attempt to detect features 
    /// </summary>
    public class FeatureDetectionResult
    {
        /// <summary>
        /// Path of project that was searched for features
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// Collection of features and whether or not they were found in a project
        /// </summary>
        public Dictionary<string, bool> FeatureStatus { get; }

        /// <summary>
        /// All features that were searched for in the detection attempt
        /// </summary>
        public IEnumerable<string> FeatureCohort => FeatureStatus.Keys;

        /// <summary>
        /// Collection of features that were present in the project
        /// </summary>
        public IEnumerable<string> PresentFeatures =>
            FeatureCohort.Where(f => FeatureStatus[f]);

        /// <summary>
        /// Collection of features that were not present in the project
        /// </summary>
        public IEnumerable<string> AbsentFeatures =>
            FeatureCohort.Where(f => !FeatureStatus[f]);

        public FeatureDetectionResult()
        {
            FeatureStatus = new Dictionary<string, bool>();
        }
    }
}
