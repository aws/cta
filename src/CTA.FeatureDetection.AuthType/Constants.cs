using System.Data;

namespace CTA.FeatureDetection.AuthType
{
    internal class Constants
    {
        internal const string ConfigurationElement = "configuration";
        internal const string SystemWebElement = "system.web";
        internal const string AuthenticationElement = "authentication";
        internal const string AuthorizationElement = "authorization";
        internal const string AllowElement = "allow";
        internal const string IdentityElement = "identity";
        internal const string ModeAttribute = "mode";
        internal const string RolesAttribute = "roles";
        internal const string ImpersonateAttribute = "impersonate";
        internal const string WindowsAuthenticationType = "Windows";
        internal const string FormsAuthenticationType = "Forms";
        internal const string FederatedAuthenticationType = "Federated";
        internal const string AuthorizeMethodAttribute = "Authorize";
        internal const string RolesAttributeArgument = "Roles";
    }
}
