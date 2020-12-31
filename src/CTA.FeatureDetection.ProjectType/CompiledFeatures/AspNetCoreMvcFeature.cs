using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class AspNetCoreMvcFeature : CompiledFeature
    {
        /// <summary>
        /// Determines that a project is an ASP.NET Core MVC project if:
        ///     1) Any class derived from the Controller abstract class also calls any method returning a view-related object
        ///     AND
        ///     2) The project contains a non-empty Views directory
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is an ASP.NET Core MVC project or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var project = analyzerResult.ProjectResult;
            var controllerClassDeclarations = project.GetClassDeclarationsWithBaseType(Constants.NetCoreMvcControllerOriginalDefinition);
            var returnsViewObjectFromControllerClasses = controllerClassDeclarations.Any(c => c.GetInvocationExpressionsBySemanticReturnType(Constants.NetCoreViewResultTypes).Any());

            var isPresent = returnsViewObjectFromControllerClasses
                && project.ContainsNonEmptyDirectory(Constants.MvcViewsDirectory);

            return isPresent;
        }
    }
}
