using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Reporting;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class WebClassLibraryFeature : CompiledFeature
    {
        public override FeatureCategory FeatureCategory => FeatureCategory.ProjectType;

        public override string Description => "This project is a Web class library.";

        public override bool IsLinuxCompatible => true;

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
