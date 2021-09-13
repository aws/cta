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
        public const string WebConfig = "web.config";
        public const string Name = "name";
        public const string ConnectionstringsLower = "connectionstrings";
        public const string Connectionstring = "connectionstring";
        public const string Key = "key";
        public const string Value = "value";
        public const string ConnectionStrings = "ConnectionStrings";
        public const string nginxConfig = "nginx.json";

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
            new List<string> {"webapi","nginx.json"},
            new List<string> {"mvc","nginx.json"},
        };

        // nginx attributes needed for deployment
        public static readonly Dictionary<string, List<string>> nginxBaseAttributes = new Dictionary<string, List<string>>()
        {
            {"proxy_pass", new List<string>{"http://127.0.0.1:5000;" } },
            {"proxy_set_header", new List<string>{"Host $host", "X-Real-IP $remote_addr", "X-Forwarded-For $proxy_add_x_forwarded_for", "X-Forwarded-Proto $scheme"}},
            {"client_max_body_size", new List<string>{ "10m" }}
        };
        

    }
}
