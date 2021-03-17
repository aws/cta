using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
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
        /// 2. IAppBuilder.UseWsFederationAuthentication(...)
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Federated Authentication is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInConfig(analyzerResult) || IsPresentInCode(analyzerResult);
        }

        public bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var config = WebConfigManager.LoadWebConfigAsXDocument(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsAttributeWithValue(Constants.AuthenticationElementElementPath, Constants.ModeAttribute, Constants.FederatedAuthenticationType);
        }

        public bool IsPresentInCode(AnalyzerResult analyzerResult)
        {
            return analyzerResult.ProjectResult.SourceFileResults.Any(s => 
                s.AllInvocationExpressions().Any(i => i.SemanticOriginalDefinition.StartsWith(Constants.WsFederationAuthenticationQualifiedName)));
        }
    }
}
