using System.Text.RegularExpressions;
using Codelyzer.Analysis.Build;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class ProjectBuildResultExtensions
    {
        public static bool IsDotnetFramework(this ProjectBuildResult projectBuildResult)
        {
            var targetFrameworksValue = GetTargetFrameworksValue(projectBuildResult);

            return IsRegexMatch(Constants.DotnetFrameworkPattern, targetFrameworksValue)
                   || IsRegexMatch(Constants.DotnetFrameworkSdkPattern, targetFrameworksValue);
        }

        public static bool IsDotnetCore(this ProjectBuildResult projectBuildResult)
        {
            var targetFrameworksValue = GetTargetFrameworksValue(projectBuildResult);

            return IsRegexMatch(Constants.DotnetCoreAppPattern, targetFrameworksValue)
                   || IsRegexMatch(Constants.DotnetCorePattern, targetFrameworksValue);
        }

        public static bool IsDotnetStandard(this ProjectBuildResult projectBuildResult)
        {
            var targetFrameworksValue = GetTargetFrameworksValue(projectBuildResult);

            return IsRegexMatch(Constants.DotnetStandardPattern, targetFrameworksValue);
        }

        private static string GetTargetFrameworksValue(ProjectBuildResult projectBuildResult)
        {
            return projectBuildResult.TargetFramework ?? string.Empty;
        }

        private static bool IsRegexMatch(string regexPattern, string textToMatch)
        {
            var regex = new Regex(regexPattern);
            return regex.Match(textToMatch).Success;
        }
    }
}
