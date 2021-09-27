using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.Common.Models.Features.Base
{
    public abstract class WebConfigFeature : CompiledFeature
    {
        protected WebConfigXDocument LoadWebConfig(string projectDir)
        {
            return WebConfigManager.LoadWebConfigAsXDocument(projectDir);
        }
    }
}
