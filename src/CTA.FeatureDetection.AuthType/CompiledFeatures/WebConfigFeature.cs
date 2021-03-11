using System;
using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public abstract class WebConfigFeature : CompiledFeature
    {
        protected WebConfigXDocument LoadWebConfig(string projectDir)
        {
            return WebConfigManager.LoadWebConfigAsXDocument(projectDir);
        }
    }
}
