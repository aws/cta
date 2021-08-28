namespace CTA.FeatureDetection.Common.Reporting
{
    public class FeatureReportRecord
    {
        public FeatureCategory FeatureCategory { get; set; }

        public string ProjectName { get; set; }

        public string FeatureName { get; set; }

        public string Description { get; set; }

        public bool IsLinuxCompatible { get; set; }
    }
}
