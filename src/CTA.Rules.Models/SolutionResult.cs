using System.Collections.Concurrent;

namespace CTA.Rules.Models
{
    public class SolutionResult
    {
        public SolutionResult()
        {
            ProjectResults = new BlockingCollection<ProjectResult>();
        }
        public BlockingCollection<ProjectResult> ProjectResults;
    }
}
