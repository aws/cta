using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using Codelyzer.Analysis.Model;
using CTA.Rules.Models;

namespace CTA.Rules.Update
{
    public class DefaultProjectRewriterFactory : IProjectRewriterFactory
    {
        public ProjectRewriter GetInstance(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
        {
            return new ProjectRewriter(analyzerResult, projectConfiguration);
        }

        public ProjectRewriter GetInstance(IDEProjectResult ideProjectResult, ProjectConfiguration projectConfiguration)
        {
            return new ProjectRewriter(ideProjectResult, projectConfiguration);
        }
    }
}

