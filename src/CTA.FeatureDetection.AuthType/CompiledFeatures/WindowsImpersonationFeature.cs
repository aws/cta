using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsImpersonationFeature : WebConfigFeature
    {
        private readonly string _path = $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.IdentityElement}";

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
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c => c.ContainsAttributeValue(_path, Constants.ImpersonateAttribute, true.ToString().ToLower()));
        }
    }
}
