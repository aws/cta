using CTA.Rules.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Test
{
    public class TestSolutionAnalysis
    {
        public List<ProjectResult> ProjectResults { get; set; }
        public string SolutionAnalysisResult {get; set;}
        public PortSolutionResult SolutionRunResult { get; set; }

        public TestSolutionAnalysis()
        {
            ProjectResults = new List<ProjectResult>();
            SolutionAnalysisResult = string.Empty;
        }
    }

    public class ProjectResult
    {
        public string ProjectAnalysisResult { get; set; }
        public string CsProjectPath { get; set; }
        public string CsProjectContent { get; set; }
        public string ProjectDirectory { get; set; }
    }
}
