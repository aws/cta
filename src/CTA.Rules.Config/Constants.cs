using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        public static string ResourcesFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "resources.zip"));
        public static string DefaultCoreVersion = "netcoreapp3.1";

        public static string JsonFileSchema = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine(new string[] { "..", "..", "..", "..", ".." })
        , Path.Combine(new string[] { "src", "CTA.Rules.RuleFiles", "Schema", "schema.validator.json" })));

        public const string S3RootUrl = "https://s3.us-west-2.amazonaws.com/aws.portingassistant.dotnet.datastore";
        public const string S3RecommendationsBucketUrl = S3RootUrl + "/recommendationsync/recommendation";
        public const string S3CTAFiles = S3RootUrl + "/ctafiles/resources.zip";

        public const int ThreadCount = 10;
        public const int MaxRecursionDepth = 100000;
        public const int DefaultThreadSleepTime = 2000;
        public const int CacheExpiryHours = 1;


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
        public const string wwwroot = "wwwroot";
        public const string Content = "Content";
        public const string Scripts = "Scripts";

        //Config migration
        public const string appSettingsJson = "appsettings.json";
        public const string appSettings = "appsettings";
        public const string webConfig = "web.config";
        public const string name = "name";
        public const string connectionstrings = "connectionstrings";
        public const string connectionstring = "connectionstring";
        public const string key = "key";
        public const string value = "value";
        public const string ConnectionStrings = "ConnectionStrings";
    }
}
