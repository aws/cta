using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;
using System.Linq;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class AspNetCoreWebApiFeature : CompiledFeature
    {
        /// <summary>
        /// Determines that a project is an ASP.NET Core WebAPi project if:
        ///     1) There are any classes derived from the ControllerBase abstract class
        ///        and no classes derived from the Controller abstract class
        ///     OR
        ///     2) If there are any classes derived from the Controller abstract class,
        ///        none of them call any methods that return a view-related object.
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is an ASP.NET Core WebApi project or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var project = analyzerResult.ProjectResult;
            var controllerBaseClassDeclarations = project.GetClassDeclarationsWithBaseType(Constants.NetCoreMvcControllerBaseOriginalDefinition);
            var controllerClassDeclarations = project.GetClassDeclarationsWithBaseType(Constants.NetCoreMvcControllerOriginalDefinition);
            var returnsViewObjectFromControllerClasses = controllerClassDeclarations.Any(c => c.GetInvocationExpressionsBySemanticReturnType(Constants.NetCoreViewResultTypes).Any());

            var isPresent = (controllerBaseClassDeclarations.Any() && !controllerClassDeclarations.Any())
                            || (controllerClassDeclarations.Any() && !returnsViewObjectFromControllerClasses);

            return isPresent;
        }
    }
}