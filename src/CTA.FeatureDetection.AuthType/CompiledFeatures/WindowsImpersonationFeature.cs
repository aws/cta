using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsImpersonationFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if Windows Impersonation is being used in a given project based on
        /// Web.config settings.
        ///
        /// Qualifications:
        /// 1. Web.config uses windows impersonation:
        ///    <configuration>
        ///      <system.web>
        ///        <identity impersonate="true">
        ///        </identity>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Windows Impersonation is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var config = LoadWebConfig(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsAttributeWithValue(Constants.IdentityElementPath, Constants.ImpersonateAttribute, true.ToString().ToLower());
        }
    }
}
