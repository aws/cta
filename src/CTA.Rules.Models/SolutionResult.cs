using System.Collections.Concurrent;
using System.Collections.Generic;
using CTA.Rules.Models.Metrics;

namespace CTA.Rules.Models
{
    public class SolutionResult
    {
        public BlockingCollection<ProjectResult> ProjectResults { get; set; }
        public IEnumerable<CTAMetric> CTAMetrics { get; set; }

        public SolutionResult()
        {
            ProjectResults = new BlockingCollection<ProjectResult>();
            CTAMetrics = new List<CTAMetric>();
        }
    }
}
