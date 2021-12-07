using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Metrics;

namespace CTA.WebForms2Blazor.Metrics
{
    public class WebFormMetricContext
    {
        private readonly MetricsContext _metricContext;
        private readonly string _projectPath;
        private BlockingCollection<ClassConversionMetric> ClassConversionMetrics { get; set; }
        private BlockingCollection<DirectiveConversionMetric> DirectiveConversionMetrics { get; set; }
        private BlockingCollection<FileConversionMetric> FileConversionMetrics { get; set; }
        private BlockingCollection<ControlConversionMetric> ControlConversionMetrics { get; set; }
        public WebFormMetricContext(MetricsContext metricsContext, string projectPath)
        {
            _metricContext = metricsContext;
            _projectPath = projectPath;
            ClassConversionMetrics = new BlockingCollection<ClassConversionMetric>();
            DirectiveConversionMetrics = new BlockingCollection<DirectiveConversionMetric>();
            FileConversionMetrics = new BlockingCollection<FileConversionMetric>();
            ControlConversionMetrics = new BlockingCollection<ControlConversionMetric>();
        }

        public void CollectControlConversionMetrics(string childActionName, string nodeName)
        {
            var metric = new ControlConversionMetric(_metricContext, childActionName, _projectPath, nodeName);
            ControlConversionMetrics.Add(metric);
        }
        public void CollectFileConversionMetrics(string childActionName)
        {
            var metric = new FileConversionMetric(_metricContext, childActionName, _projectPath);
            FileConversionMetrics.Add(metric);
        }
        public void CollectDirectiveConversionMetrics(string childActionName)
        {
            var metric = new DirectiveConversionMetric(_metricContext, childActionName, _projectPath);
            DirectiveConversionMetrics.Add(metric);
        }
        public void CollectClassConversionMetrics(string childActionName)
        {
            var metric = new ClassConversionMetric(_metricContext, childActionName, _projectPath);
            ClassConversionMetrics.Add(metric);
        }

        public List<CTAMetric> GetMetricList()
        {
            var allMetrics = new List<CTAMetric>(); 
            allMetrics = allMetrics
                .Concat(ClassConversionMetrics)
                .Concat(ControlConversionMetrics)
                .Concat(FileConversionMetrics)
                .Concat(DirectiveConversionMetrics)
                .ToList();
            return allMetrics;
        }
    }
}
