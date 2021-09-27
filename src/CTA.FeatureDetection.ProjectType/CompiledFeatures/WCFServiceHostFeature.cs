using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class WCFServiceHostFeature : CompiledFeature
    {
        /// <summary>
        /// Determines if a project has ServiceHost References.
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project has ServiceHost features or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var projectWorkspace = analyzerResult.ProjectResult;

            var serviceHostDeclarations = projectWorkspace.GetObjectCreationExpressionBySemanticClassType(Constants.ServiceHostClass);

            if (!serviceHostDeclarations.IsNullOrEmpty())
            {
                return true;
            }

            var containsSvcFile = projectWorkspace.ContainsFileWithExtension(Constants.SvcExtension, true);

            return containsSvcFile;
        }
    }
}
