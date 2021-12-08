using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Models;

namespace CTA.Rules.Update
{
    public class ProjectRewriterFactory
    {
        public static ProjectRewriter GetInstance(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
        {
            var projectType = projectConfiguration.ProjectType;
            var projectRewriter = projectType switch
            {
                //ProjectType.WCFConfigBasedService => new WCFProjectRewriter(analyzerResult, projectConfiguration),
                //ProjectType.WCFCodeBasedService => new WCFProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.WebForms => new WebFormsProjectRewriter( analyzerResult, projectConfiguration),
                _ => new ProjectRewriter(analyzerResult, projectConfiguration)
            };
            return projectRewriter;
        }

        public static ProjectRewriter GetInstance(IDEProjectResult ideProjectResult, ProjectConfiguration projectConfiguration)
        {
            var projectType = projectConfiguration.ProjectType;
            var projectRewriter = projectType switch
            {
                //ProjectType.WCFConfigBasedService => new WCFProjectRewriter(ideProjectResult, projectConfiguration),
                //ProjectType.WCFCodeBasedService => new WCFProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.WebForms => new WebFormsProjectRewriter(ideProjectResult, projectConfiguration),
                _ => new ProjectRewriter(ideProjectResult, projectConfiguration)
            };
            return projectRewriter;
        }
    }
}
