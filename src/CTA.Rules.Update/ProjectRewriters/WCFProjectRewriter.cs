using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Analyzer;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.RuleFiles;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class WCFProjectRewriter : ProjectRewriter
    {
        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public WCFProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
            : base(analyzerResult, projectConfiguration)
        {
        }

        public WCFProjectRewriter(IDEProjectResult projectResult, ProjectConfiguration projectConfiguration)
            : base(projectResult, projectConfiguration)
        {
        }

    }
}
