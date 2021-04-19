namespace CTA.FeatureDetection.Common
{
    internal class Constants
    {
        // Target Framework Patterns
        internal const string DotnetStandardPattern = @"netstandard\d\.\d"; // Example: netstandard0.0
        internal const string DotnetFrameworkPattern = @"v\d[\.\d]{1,2}";   // Example: v0.0, v0.0.0
        internal const string DotnetFrameworkSdkPattern = @"net[\d]{2,3}";  // Example: net00, net000
        internal const string DotnetCoreAppPattern = @"netcoreapp\d\.\d";   // Example: netcoreapp0.0
        internal const string DotnetCorePattern = @"net\d\.\d";             // Example: net0.0
    }
}
