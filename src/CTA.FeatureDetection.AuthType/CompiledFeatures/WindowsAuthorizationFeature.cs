using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationFeature : WebConfigFeature
    {
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInConfig(analyzerResult) || IsPresentInCode(analyzerResult);
        }

        private bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c =>
                c.RootSectionGroup?.SectionGroups[Constants.AuthorizationElement]?.Sections[Constants.AllowElement] != null
                || c.RootSectionGroup?.SectionGroups[Constants.AuthorizationElement]?.SectionGroups[Constants.AllowElement] != null
                || c.RootSectionGroup?.SectionGroups[Constants.SystemWebElement]?.Sections[Constants.AuthorizationElement] != null
                || c.RootSectionGroup?.SectionGroups[Constants.SystemWebElement]?.SectionGroups[Constants.AuthorizationElement] != null
                || c.RootSectionGroup?.SectionGroups[Constants.ConfigurationElement]?.SectionGroups[Constants.SystemWebElement]?.Sections[Constants.AuthorizationElement] != null
                || c.RootSectionGroup?.SectionGroups[Constants.ConfigurationElement]?.SectionGroups[Constants.SystemWebElement]?.SectionGroups[Constants.AuthorizationElement] != null);
        }

        private bool IsPresentInCode(AnalyzerResult analyzerResult)
        {
            var allAttributes = analyzerResult.ProjectResult.SourceFileResults.SelectMany(r => r.AllAnnotations());
            return allAttributes.Any(a => a.Identifier == Constants.AuthorizeMethodAttribute);
        }
    }
}
