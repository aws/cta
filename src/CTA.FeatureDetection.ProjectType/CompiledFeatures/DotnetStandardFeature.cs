using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class DotnetStandardFeature : CompiledFeature
    {
        /// <summary>
        /// Determines that a project is .NET Core if it matches the target framework naming pattern
        /// used for .NET Standard runtimes.
        ///
        /// Note: this does not search for correctness, only convention.
        /// 
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether or not a project is .NET Standard</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return analyzerResult.ProjectBuildResult.IsDotnetStandard();
        }
    }
}
