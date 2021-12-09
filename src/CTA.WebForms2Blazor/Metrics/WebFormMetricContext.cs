using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Models;

namespace CTA.WebForms2Blazor.Metrics
{
    public class WebFormMetricContext
    {
        private BlockingCollection<WebFormMetric> WebFormMetrics { get; set; }

        public WebFormMetricContext()
        {
            WebFormMetrics = new BlockingCollection<WebFormMetric>();
        }

        public void CollectActionMetrics(WebFormsActionType actionName, string childActionName, string nodeName = null)
        {
            var metric = new WebFormMetric(actionName, childActionName, nodeName);
            WebFormMetrics.Add(metric);
        }

        public List<WebFormsMetricResult> Transform()
        {
            var metrics = new List<WebFormsMetricResult>();
            foreach (var metric in WebFormMetrics)
            {
                metrics.Add(new WebFormsMetricResult()
                {
                    ActionName = metric.ActionName,
                    ChildAction = metric.ChildActionName,
                    NodeName = metric.NodeName
                });

            }
            return metrics;
        }
    }
}
