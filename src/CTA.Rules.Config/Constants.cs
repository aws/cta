using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CTA.Rules.Config
{
    /// <summary>
    /// Constants used in the Rules engine
    /// </summary>
    public class Constants
    {
        //To be replaced by project namespace
        public const string NameSpacePlaceHolder = "#NAMESPACEPLACEHOLDER#";
        public const string Translator = "Translator";

        public static List<string> appSettingsExclusions = new List<string> { "webpages:Version" };
        public static List<string> excludedDirs = new List<string> { @"obj\Debug", @"bin\Debug" };
        public static List<string> excludedPatterns = new List<string> { "Startup.cs", "AssemblyInfo.cs" };
        public static HashSet<string> ExcludedBaseClasses = new HashSet<string>() { "object" };

        public static List<string> filesToArchive = new List<string> { "BundleConfig.cs", "FilterConfig.cs", "RouteConfig.cs", "WebApiConfig.cs", "global.asax.*", "AssemblyInfo.cs", "packages.config", "Startup.cs", "Program.cs" };

        public static HashSet<string> CoreVersions = new HashSet<string>() { "netcoreapp3.1" };

        public static string RulesDefaultPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Input"));
        public static string DefaultFeaturesFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates", "default.json"));
        public static string ResourcesFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "resources.zip"));
        public static string ResourcesExtractedPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates"));
        public static string DefaultCoreVersion = "netcoreapp3.1";

        public static string JsonFileSchema = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine(new string[] { "..", "..", "..", "..", ".." })
        , Path.Combine(new string[] { "src", "CTA.Rules.RuleFiles", "Schema", "schema.validator.json" })));

        public const string S3RootUrl = "https://s3.us-west-2.amazonaws.com/aws.portingassistant.dotnet.datastore";
        public const string S3RecommendationsBucketUrl = S3RootUrl + "/recommendationsync/recommendation";
        public const string S3TemplatesBucketUrl = S3RootUrl + "/recommendationsync/Templates";
        

        public const int ThreadCount = 10;
        public const int MaxRecursionDepth = 100000;
        public const int DefaultThreadSleepTime = 2000;
        public const int CacheExpiryHours = 1;
        public const int CacheExpiryDays = 1;


        public const int DownloadRetryCount = 3;

        public const string Templates = "Templates";

        public const string CommentFormat = @"/* Added by CTA: {0} */";
        public const string WebSdkName = "Microsoft.NET.Sdk.Web";
        public const string ClassLibrarySdkName = "Microsoft.NET.Sdk";

        public const string Identifier = "Identifier";
        public const string BaseClass = "BaseClass";
        public const string ClassName = "ClassName";
        public const string InterfaceName = "InterfaceName";

        public const string PackageName = "Name";
        public const string PackageVersion = "Version";

        public const string Namespace = "Namespace";
        public const string Full = "Full";
        public const string Project = "Project";

        public const string AttributeActions = "AttributeActions";
        public const string AttributeListActions = "AttributeListActions";
        public const string ClassActions = "ClassActions";
        public const string CompilationUnitActions = "CompilationUnitActions";
        public const string IdentifierNameActions = "IdentifierNameActions";
        public const string InvocationExpressionActions = "InvocationExpressionActions";
        public const string ExpressionActions = "ExpressionActions";
        public const string MethodDeclarationActions = "MethodDeclarationActions";
        public const string MemberAccessActions = "MemberAccessActions";
        public const string ElementAccessActions = "ElementAccessActions";
        public const string NamespaceActions = "NamespaceActions";
        public const string ObjectCreationExpressionActions = "ObjectCreationExpressionActions";
        public const string ProjectLevelActions = "ProjectLevelActions";
        public const string InterfaceActions = "InterfaceActions";
        public const string ProjectFileActions = "ProjectFileActions";

        public const string ProjectRecommendationFile = "project.all";

        public const string MetricsTag = "CTA_METRICS_TAG";

        //Folder migration
        public const string Wwwroot = "wwwroot";
        public const string Content = "Content";
        public const string Scripts = "Scripts";

        //Config migration
        public const string AppSettingsJson = "appsettings.json";
        public const string AppSettings = "appsettings";
        public const string AppConfig = "App.config";
        public const string WebConfig = "web.config";
        public const string Name = "name";
        public const string ConnectionstringsLower = "connectionstrings";
        public const string Connectionstring = "connectionstring";
        public const string Key = "key";
        public const string Value = "value";
        public const string ConnectionStrings = "ConnectionStrings";

        public static readonly List<string> SupportedMethodModifiers = new List<string>() { "public", "internal", "protected", "private", "abstract", "extern", "override", "static", "unsafe", "virtual", "async" };


        public static readonly List<List<string>> TemplateFiles = new List<List<string>>() {
            new List<string> {"default.json"},
            new List<string> {"classlibrary","appsettings.json"},
            new List<string> {"webapi","appsettings.json"},
            new List<string> {"webapi","Program.cs"},
            new List<string> {"webapi","Startup.cs"},
            new List<string> {"mvc","appsettings.json"},
            new List<string> {"mvc","Program.cs"},
            new List<string> {"mvc","Startup.cs" },
            new List<string> {"webclasslibrary","appsettings.json" },
            new List<string> {"wcfservice", "Program.cs"},
            new List<string> {"wcfservice", "Startup.cs"}
        };

        //WCF Constants

        public const string CoreWCFRule = "CoreWCF";
        public const string CoreWCFChannelsRule = "CoreWCF.Channels";
        public const string CoreWCFSecurityRule = "CoreWCF.Security";
        public const string CoreWCFDispatcherRule = "CoreWCF.Dispatcher";
        public const string CoreWCFDescriptionRule = "CoreWCF.Description";
        public const string CoreWCFDiagnosticsRule = "CoreWCF.Diagnostics";
        public const string CoreWCFSecurityTokenRule = "CoreWCF.Security.Token";
        public const string CoreWCFIdentityModelRule = "CoreWCF.IdentityModel";
        public const string CoreWCFIdentityModelTokensRule = "CoreWCF.IdentityModel.Tokens";
        public const string CoreWCFIdentityModelClaimsRule = "CoreWCF.IdentityModel.Claims";
        public const string CoreWCFIdentityModelPolicyRule = "CoreWCF.IdentityModel.Policy";
        public const string CoreWCFIdentityModelSelectorsRule = "CoreWCF.IdentityModel.Selectors";
        public const string CoreWCFIdentityModelServicesTokensRule = "CoreWCF.IdentityModel.Services.Tokens";
        public const string CoreWCFConfigurationRule = "CoreWCF.Configuration";
        public const string CoreWCFWebRule = "CoreWCF.Web";
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
        public const string AddServiceEndpointFormat = "\nbuilder.AddServiceEndpoint<{0}, {1}>({2}, \"{3}\");";

        public const string DefaultServiceInterface = "IService";
        public const string DefaultServiceClass = "Service";

        public const string HttpProtocol = "basichttpbinding";
        public const string NettcpProtocol = "nettcpbinding";
        public const string WSHttpProtocol = "wshttpbinding";
        public const string HttpsProtocol = "basichttpsbinding";
        public const string NethttpProtocol = "nethttpbinding";
        public const string MexBinding = "mexHttpBinding";
        public const string TransportMessageCredentialsMode = "TransportWithMessageCredential";

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
