namespace CTA.FeatureDetection.AuthType
{
    internal class Constants
    {
        internal const string ConfigurationElement = "configuration";
        internal const string SystemWebElement = "system.web";
        internal const string SystemWebServerElement = "system.webServer";
        internal const string AuthenticationElement = "authentication";
        internal const string AuthorizationElement = "authorization";
        internal const string AllowElement = "allow";
        internal const string SecurityElement = "security";
        internal const string WindowsAuthenticationElement = "windowsAuthentication";
        internal const string IdentityElement = "identity";
        internal const string ModeAttribute = "mode";
        internal const string RolesAttribute = "roles";
        internal const string ImpersonateAttribute = "impersonate";
        internal const string WindowsAuthenticationType = "Windows";
        internal const string FormsAuthenticationType = "Forms";
        internal const string FederatedAuthenticationType = "Federated";
        internal const string AuthorizeMethodAttribute = "Authorize";
        internal const string RolesAttributeArgument = "Roles";
        internal const string WsFederationAuthenticationQualifiedName = "Owin.IAppBuilder.UseWsFederationAuthentication";

        // Paths
        internal static readonly string AuthenticationElementElementPath = $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.AuthenticationElement}";
        internal static readonly string AuthorizationElementPath = $"{ConfigurationElement}/{SystemWebElement}/{AuthorizationElement}";
        internal static readonly string AllowElementPath = $"{ConfigurationElement}/{SystemWebElement}/{AuthorizationElement}/{AllowElement}";
        internal static readonly string IdentityElementPath = $"{ConfigurationElement}/{SystemWebElement}/{IdentityElement}";
    }
}
