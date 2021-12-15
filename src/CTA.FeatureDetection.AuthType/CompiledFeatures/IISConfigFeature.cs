using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class IISConfigFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if IIS server configuration is being used in a given project based on
        /// Web.config settings.
        ///
        /// Qualifications:
        /// 1. Web.config uses IIS server config:
        ///   <configuration>
        ///      <system.webServer>
        ///        <authentication>
        ///        </authentication>
        ///      </system.webServer>
        ///   </configuration>
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not IIS Server configuration is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var config = LoadWebConfig(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsElement(Constants.IISConfigElementPath);
        }
    }
}
