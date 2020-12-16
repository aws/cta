using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

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
