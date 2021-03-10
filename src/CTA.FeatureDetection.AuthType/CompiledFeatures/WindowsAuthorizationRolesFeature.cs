using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationRolesFeature : WebConfigFeature
    {
        private readonly string _path = $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.AuthorizationElement}/{Constants.AllowElement}";

        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInCode(analyzerResult) || IsPresentInConfig(analyzerResult);
        }

        private bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c => c.ContainsAttribute(_path, Constants.RolesAttribute));
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
