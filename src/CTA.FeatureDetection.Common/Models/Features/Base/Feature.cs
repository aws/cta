using System;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Models.Enums;

namespace CTA.FeatureDetection.Common.Models.Features.Base
{
    public abstract class Feature
    {
        /// <summary>
        /// Name of the feature
        /// </summary>
        public virtual string Name { get; set; }

        public FeatureScope FeatureScope { get; set; }

        /// <summary>
        /// Determines whether or not the feature is present in a given project
        /// </summary>
        /// <param name="analyzerResult">Source code analysis result</param>
        /// <returns>Whether or not the feature was found</returns>
        public abstract bool IsPresent(AnalyzerResult analyzerResult);

        protected bool Equals(Feature other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((Feature)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
