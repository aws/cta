using System.Collections.Generic;

namespace CTA.Rules.Update.WCF
{
    public class Constants
    {
        //WCF Constants
        public static readonly List<string> CoreWCFRules = new List<string>
        {
            "CoreWCF",
            "CoreWCF.Channels",
            "CoreWCF.Security",
            "CoreWCF.Dispatcher",
            "CoreWCF.Description",
            "CoreWCF.Diagnostics",
            "CoreWCF.Security.Token",
            "CoreWCF.IdentityModel",
            "CoreWCF.IdentityModel.Tokens",
            "CoreWCF.IdentityModel.Claims",
            "CoreWCF.IdentityModel.Policy",
            "CoreWCF.IdentityModel.Selectors",
            "CoreWCF.IdentityModel.Services.Tokens",
            "CoreWCF.Configuration",
            "CoreWCF.Web",
        };

        public const string CoreWCFConfigBasedProjectRule = "CoreWCF.ConfigurationBased.Project";
        public const string CoreWCFCodeBasedProjectRule = "CoreWCF.CodeBased.Project";

        public const string WCFClientProjectRule = "WCF.Client";

        public const string PortedConfigFileName = "corewcf_ported.config";
        public const string ConfigXMLVersion = "1.0";
        public const string ConfigXMLEncoding = "utf-16";
        public const string ConfigXMLStandalone = "yes";

        public const string SystemServiceModelElement = "system.serviceModel";
        public const string HostElement = "host";
        public const string EndpointElement = "endpoint";
        public const string ConfigurationElement = "configuration";
        public const string BehaviorsElement = "behaviors";
        public static readonly string BehaviorsPath = $"{ConfigurationElement}/{SystemServiceModelElement}/{BehaviorsElement}";
        public const string BindingAttribute = "binding";

        public const string XMLPathPlaceholder = "#XMLPATH#";

        public const string AddServiceFormat = "builder.AddService <{0}>();";
        public const string AddServiceEndpointFormat = "\nbuilder.AddServiceEndpoint<{0}, {1}>({2}, {3});";

        public const string DefaultServiceInterface = "IService";
        public const string DefaultServiceClass = "Service";

        public const string HttpProtocol = "basichttpbinding";
        public const string NettcpProtocol = "nettcpbinding";
        public const string WSHttpProtocol = "wshttpbinding";
        public const string HttpsProtocol = "basichttpsbinding";
        public const string NethttpProtocol = "nethttpbinding";
        public const string MexBinding = "mexHttpBinding";
        public const string TransportMessageCredentialsMode = "TransportWithMessageCredential";

        public const int HttpDefaultPort = 8080;
        public const int HttpsDefaultPort = 8888;
        public const int NetTcpDefaultPort = 8000;

        public const string EndpointPlaceholder = "#ENDPOINTPLACEHOLDER#";
        public const string WCFConfigManagerAPI = "AddServiceModelConfigurationManagerFile";
        public const string WCFBehaviorsMessage = "  // The API does not support behaviors section inside config. Please modify the configure method for service behaviors support. Refer to https://github.com/CoreWCF/CoreWCF\n";
        public const string ListenLocalHostFormat = "\n{0}.ListenLocalhost({1});";
        public const string ListenHttpsFormat = @"
        {0}.Listen(address: IPAddress.Loopback, {1}, listenOptions =>
        {{
            listenOptions.UseHttps(httpsOptions =>
            {{
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            }});
        }});";
        public const string NetTcpFormat = @"UseNetTcp";


        public const string UseStartupMethodIdentifier = "UseStartup";
    }
}
