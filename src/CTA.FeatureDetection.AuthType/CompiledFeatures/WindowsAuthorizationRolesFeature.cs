using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationRolesFeature : WebConfigFeature
    {
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInConfig(analyzerResult) || IsPresentInCode(analyzerResult);
        }

        private bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c =>
                c.RootSectionGroup?.SectionGroups[Constants.AuthorizationElement]?.Sections[Constants.AllowElement]?.ElementInformation.Properties[Constants.RolesAttribute] != null
                || c.RootSectionGroup?.SectionGroups[Constants.SystemWebElement]?.SectionGroups[Constants.AuthorizationElement]?.Sections[Constants.AllowElement]?.ElementInformation.Properties[Constants.RolesAttribute] != null
                || c.RootSectionGroup?.SectionGroups[Constants.ConfigurationElement]?.SectionGroups[Constants.SystemWebElement]?.SectionGroups[Constants.AuthorizationElement]?.Sections[Constants.AllowElement]?.ElementInformation.Properties[Constants.RolesAttribute] != null);
        }
        
        private bool IsPresentInCode(AnalyzerResult analyzerResult)
        {
            var allAttributes = analyzerResult.ProjectResult.SourceFileResults.SelectMany(r => r.AllAnnotations());
            var authorizeAttributes = allAttributes.Where(a => a.Identifier == Constants.AuthorizeMethodAttribute);
            
            return authorizeAttributes.SelectMany(a => a.AllAttributeArguments())
                .Any(a => a.ArgumentName == Constants.RolesAttributeArgument);
        }
    }
}
