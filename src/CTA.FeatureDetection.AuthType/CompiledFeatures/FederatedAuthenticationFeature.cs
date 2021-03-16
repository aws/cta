using Codelyzer.Analysis;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class FederatedAuthenticationFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if Federated Authentication is being used in a given project based on
        /// Web.config settings and method invocations in code.
        ///
        /// Qualifications:
        /// 1. Web.config uses federated authentication:
        ///    <configuration>
        ///      <system.web>
        ///        <authentication mode="Federated">
        ///        </authentication>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Federated Authentication is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var config = WebConfigManager.LoadWebConfigAsXDocument(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsAttributeWithValue(Constants.AuthenticationElementElementPath, Constants.ModeAttribute, Constants.FederatedAuthenticationType);
        }
    }
}
