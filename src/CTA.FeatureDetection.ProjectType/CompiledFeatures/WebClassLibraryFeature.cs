using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class WebClassLibraryFeature : CompiledFeature
    {
        /// <summary>
        /// Determines if a project is using members from the System.Web namespace
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is using System.Web or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var sourceFileRootNodes = analyzerResult.ProjectResult.SourceFileResults;
            var isPresent = sourceFileRootNodes.Any(n => n.ContainsReference(Constants.SystemWebReferenceIdentifier));

            return isPresent;
        }
    }
}
