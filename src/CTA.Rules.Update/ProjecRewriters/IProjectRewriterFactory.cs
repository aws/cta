using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Models;

namespace CTA.Rules.Update
{
    public interface IProjectRewriterFactory
    {
        public ProjectRewriter GetInstance(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration);
        public ProjectRewriter GetInstance(IDEProjectResult ideProjectResult, ProjectConfiguration projectConfiguration);
    }
}
