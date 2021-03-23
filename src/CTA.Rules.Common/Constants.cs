namespace CTA.Rules.Common
{
    internal class Constants
    {
        // Target Framework Patterns
        internal const string DotnetStandardPattern = @"netstandard\d\.\d";
        internal const string DotnetFrameworkPattern = @"v\d[\.\d]{1,2}";
        internal const string DotnetFrameworkSdkPattern = @"net[\d]{2,3}";
        internal const string DotnetCoreAppPattern = @"netcoreapp\d\.\d";
        internal const string DotnetCorePattern = @"net\d\.\d";

        // XML Elements
        internal const string TargetFrameworkVersionElement = "TargetFrameworkVersion";
        internal const string TargetFrameworkElement = "TargetFramework";
        internal const string TargetFrameworksElement = "TargetFrameworks";
    }
}
