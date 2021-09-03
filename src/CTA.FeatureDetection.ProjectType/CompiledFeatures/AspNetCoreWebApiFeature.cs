using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Reporting;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class AspNetCoreWebApiFeature : CompiledFeature
    {
        public override FeatureCategory FeatureCategory => FeatureCategory.ProjectType;

        public override string Description => "This project type is ASP.NET Core Web API.";

        public override bool IsLinuxCompatible => true;

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

            var publicMethodsInControllerClasses = allControllers
                .SelectMany(c => c.GetPublicMethodDeclarations())
                .ToList();
            var publicMethodsReturningNonViewObject = publicMethodsInControllerClasses
                .SelectMany(m => m.AllReturnStatements())
                .Where(r => !Constants.NetCoreViewResultTypes.Contains(r.SemanticReturnType));
            var publicMethodsReturningNothing = publicMethodsInControllerClasses
                .Where(m => m.AllReturnStatements().IsNullOrEmpty());
            var publicMethodsNotReturningViewObject = publicMethodsReturningNonViewObject
                .Concat(publicMethodsReturningNothing);


            var isPresent = classesWithApiControllerAttribute.Any() || publicMethodsNotReturningViewObject.Any();

            return isPresent;
        }
    }
}
