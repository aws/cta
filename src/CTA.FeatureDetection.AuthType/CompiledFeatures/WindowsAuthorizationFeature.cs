﻿using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationFeature : WindowsAuthenticationFeature
    {
        /// <summary>
        /// Determines if Windows Authorization is being used in a given project based on
        /// Web.config settings
        ///
        /// Qualifications:
        /// 1. Web.config uses authorization:
        ///    <configuration>
        ///      <system.web>
        ///        <authentication mode="Windows">
        ///        </authentication>
        ///        <authorization>
        ///        </authorization>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Windows Authorization is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var config = LoadWebConfig(analyzerResult.ProjectResult.ProjectRootPath);
            return base.IsPresent(analyzerResult) && config.ContainsElement(Constants.AuthorizationElementPath);
        }
    }
}
