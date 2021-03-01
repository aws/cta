using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public class FeatureDetectionResultReportGenerator
    {
        public MetricsContext Context { get; set; }
        public Dictionary<string, FeatureDetectionResult> FeatureDetectionResults { get; set; }
        public IEnumerable<FeatureDetectionMetric> FeatureDetectionMetrics { get; set; }
        public string FeatureDetectionResultJsonReport { get; set; }

        public FeatureDetectionResultReportGenerator(MetricsContext context, Dictionary<string,FeatureDetectionResult> featureDetectionResults)
        {
            Context = context;
            FeatureDetectionResults = featureDetectionResults;
        }

        public void GenerateFeatureDetectionReport()
        {
            GenerateMetrics();
            GenerateFeatureDetectionResultJsonReport();
        }

        private void GenerateMetrics()
        {
            FeatureDetectionMetrics = MetricsTransformer.TransformFeatureDetectionResults(Context, FeatureDetectionResults);
        }
        
        private void GenerateFeatureDetectionResultJsonReport()
        {
            FeatureDetectionResultJsonReport = JsonConvert.SerializeObject(FeatureDetectionMetrics);
        }
    }
}
