using CTA.FeatureDetection.Common.WCFConfigUtils;

namespace CTA.FeatureDetection.Common.Models.WCF
{
    public class BindingConfiguration
    {
        public string Mode { get; set; } = Constants.NoneMode;
        public string EndpointAddress { get; set; } = null;
    }
}
