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
            var projectClassDeclarations = project.GetAllClassDeclarations().ToList();

            var classesWithApiControllerAttribute = projectClassDeclarations
                .Where(c => c.HasAttribute(Constants.ApiControllerAttributeType))
                .ToList();

            var classesDerivedFromControllerBaseClass = projectClassDeclarations
                .Where(c => c.HasBaseType(Constants.NetCoreMvcControllerBaseOriginalDefinition));
            var classesDerivedFromControllerClass = projectClassDeclarations
                .Where(c => c.HasBaseType(Constants.NetCoreMvcControllerOriginalDefinition));
            var allControllers = classesWithApiControllerAttribute
                .Concat(classesDerivedFromControllerBaseClass)
                .Concat(classesDerivedFromControllerClass);

            var publicMethodsInControllerClasses =
                allControllers.SelectMany(c => c.GetPublicMethodDeclarations());
            var publicMethodsReturningViewObject = publicMethodsInControllerClasses.SelectMany(m =>
                m.GetInvocationExpressionsBySemanticReturnType(Constants.NetCoreViewResultTypes));

            var isPresent = classesWithApiControllerAttribute.Any() || publicMethodsReturningViewObject.Any();

            return isPresent;
        }
    }
}