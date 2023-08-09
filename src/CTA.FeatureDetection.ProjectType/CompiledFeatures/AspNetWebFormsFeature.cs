using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class AspNetWebFormsFeature : CompiledFeature
    {
        /// <summary>
        /// Determines if a project is an ASP.NET web forms project by looking at file extensions and nuget references
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is an ASP.NET web forms project or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var project = analyzerResult.ProjectResult;
            var isPresent = project.ContainsFileWithExtension(Constants.AspxExtension, true)
                || project.ContainsDependency(Constants.WebFormsScriptManagerIdentifier) || project.ContainsDependency(Constants.WebFormsWebOptimizationIdentifier);

            return isPresent;
        }
    }
}
