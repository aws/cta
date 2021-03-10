using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationFeature : WebConfigFeature
    {
        private readonly string _path = $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.AuthorizationElement}";

        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInCode(analyzerResult) || IsPresentInConfig(analyzerResult);
        }

        private bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c => c.ContainsElementPath(_path));
        }

        private bool IsPresentInCode(AnalyzerResult analyzerResult)
        {
            var allAttributes = analyzerResult.ProjectResult.SourceFileResults.SelectMany(r => r.AllAnnotations());
            return allAttributes.Any(a => a.Identifier == Constants.AuthorizeMethodAttribute);
        }
    }
}
