using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class DotnetFrameworkFeature : CompiledFeature
    {
        /// <summary>
        /// Determines that a project is .NET Framework if it matches the target framework naming pattern
        /// used for .NET Framework runtimes.
        ///
        /// Note: this does not search for correctness, only convention.
        /// 
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether or not a project is .NET Framework</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return analyzerResult.ProjectBuildResult.IsDotnetFramework();
        }
    }
}
