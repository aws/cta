using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Reporting;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class FormsAuthenticationFeature : WebConfigFeature
    {
        public override FeatureCategory FeatureCategory => FeatureCategory.AuthType;

        public override string Description => "This project uses the Forms Authentication method.";

        public override bool IsLinuxCompatible => true;
        /// <summary>
        /// Determines if Forms Authentication is being used in a given project based on
        /// Web.config settings.
        ///
        /// Qualifications:
        /// 1. Web.config uses Forms authentication:
        ///    <configuration>
        ///      <system.web>
        ///        <authentication mode="Forms">
        ///        </authentication>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Forms Authentication is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var config = WebConfigManager.LoadWebConfigAsXDocument(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsAttributeWithValue(Constants.AuthenticationElementElementPath, Constants.ModeAttribute, Constants.FormsAuthenticationType);
        }
    }
}
