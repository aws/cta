using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Reporting;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class FormsAuthenticationWithMembershipFeature : FormsAuthenticationFeature
    {
        public override FeatureCategory FeatureCategory => FeatureCategory.AuthType;

        public override string Description => "This project uses the Forms Authentication method with membership.";

        public override bool IsLinuxCompatible => false;

        /// <summary>
        /// Determines if Forms Authentication with Membership is being used in a given project based on
        /// Web.config settings.
        ///
        /// Qualifications:
        /// 1. Web.config uses Forms authentication and Membership:
        ///    <configuration>
        ///      <system.web>
        ///        <authentication mode="Forms">
        ///        </authentication>
        ///        <membership>
        ///          ...
        ///        </membership>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Forms Authentication with Membership is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var config = WebConfigManager.LoadWebConfigAsXDocument(analyzerResult.ProjectResult.ProjectRootPath);
            return base.IsPresent(analyzerResult) && config.ContainsElement(Constants.MembershipElementPath);
        }
    }
}
