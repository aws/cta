namespace CTA.FeatureDetection.ProjectType
{
    public class Constants
    {
        public static readonly string[] NetCoreViewResultTypes = {
            "ViewComponentResult",
            "ViewResult",
            "PartialViewResult"
        };
        public const string NetCoreMvcControllerOriginalDefinition = "Microsoft.AspNetCore.Mvc.Controller";
        public const string NetCoreMvcControllerBaseOriginalDefinition = "Microsoft.AspNetCore.Mvc.ControllerBase";
        public const string SystemWebReferenceIdentifier = "System.Web";
        public const string WebApiNugetReferenceIdentifier = "Microsoft.AspNet.WebApi";
        public const string WebApiControllerOriginalDefinition = "System.Web.Http.ApiController";
        public const string MvcNugetReferenceIdentifier = "Microsoft.AspNet.Mvc";
        public const string MvcControllerOriginalDefinition = "System.Web.Mvc.Controller";
        public const string MvcViewsDirectory = "Views";

        public const string AspNetMvcFeatureName = "AspNetMvcFeature";
        public const string AspNetWebApiFeatureName = "AspNetWebApiFeature";
        public const string WebClassLibraryFeatureName = "WebClassLibraryFeature";
        public const string AspNetCoreMvcFeatureName = "AspNetCoreMvcFeature";
        public const string AspNetCoreWebApiFeatureName = "AspNetCoreWebApiFeature";
    }
}