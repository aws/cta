﻿using Codelyzer.Analysis;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthenticationFeature : WebConfigFeature
    {
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
            var config = LoadWebConfig(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsAttributeWithValue(Constants.AuthenticationElementElementPath, Constants.ModeAttribute, Constants.WindowsAuthenticationType);
        }
    }
}
