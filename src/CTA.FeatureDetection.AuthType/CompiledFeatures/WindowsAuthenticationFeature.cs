using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthenticationFeature : WebConfigFeature
    {
        private readonly string _path = $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.AuthenticationElement}";

        /// <summary>
        /// Determines if Windows Authentication is being used in a given project based on
        /// Web.config settings.
        ///
        /// Qualifications:
        /// 1. Web.config uses windows authentication:
        ///    <configuration>
        ///      <system.web>
        ///        <authentication mode="Windows">
        ///        </authentication>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Windows Authentication is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c => c.ContainsAttributeValue(_path, Constants.ModeAttribute, Constants.WindowsAuthenticationType));
        }
    }
}
