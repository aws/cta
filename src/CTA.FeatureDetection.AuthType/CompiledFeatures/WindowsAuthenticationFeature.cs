using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthenticationFeature : WebConfigFeature
    {
        private readonly string _path = $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.AuthenticationElement}";

        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var configs = LoadWebConfigs(analyzerResult);
            return configs.Any(c => c.ContainsAttributeValue(_path, Constants.ModeAttribute, Constants.WindowsAuthenticationType));
        }
    }
}
