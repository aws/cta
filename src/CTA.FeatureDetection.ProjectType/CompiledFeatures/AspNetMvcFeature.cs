using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Reporting;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class AspNetMvcFeature : CompiledFeature
    {
        public override FeatureCategory FeatureCategory => FeatureCategory.ProjectType;

        public override string Description => "This project type is ASP.NET MVC.";

        public override bool IsLinuxCompatible => true;

        /// <summary>
        /// Determines if a project is an ASP.NET MVC project by looking at nuget references
        /// and the types of controllers used.
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is an ASP.NET MVC project or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var project = analyzerResult.ProjectResult;
            var isPresent = (project.ContainsNugetDependency(Constants.MvcNugetReferenceIdentifier)
                || project.ContainsDependency(Constants.MvcReferenceIdentifier))
                && project.DeclaresClassWithBaseType(Constants.MvcControllerOriginalDefinition)
                && project.ContainsNonEmptyDirectory(Constants.MvcViewsDirectory);

            return isPresent;
        }
    }
}
