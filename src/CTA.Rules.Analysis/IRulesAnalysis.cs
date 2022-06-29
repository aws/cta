using System.Collections.Generic;
using CTA.Rules.Models;

namespace CTA.Rules.Analyzer;

public interface IRulesAnalysis
{
    /// <summary>
    /// Runs the Rules Analysis
    /// </summary>
    /// <returns></returns>
    ProjectActions Analyze();

    ProjectActions AnalyzeFiles(ProjectActions projectActions, List<string> updatedFiles);
}
