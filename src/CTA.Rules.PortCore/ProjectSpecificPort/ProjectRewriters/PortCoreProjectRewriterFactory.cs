﻿using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Models;
using CTA.Rules.Update;

namespace CTA.Rules.PortCore
{
    public class PortCoreProjectRewriterFactory : IProjectRewriterFactory
    {
        public ProjectRewriter GetInstance(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
        {
            var projectType = projectConfiguration.ProjectType;
            var projectRewriter = projectType switch
            {
                ProjectType.WCFCodeBasedService => new WCFProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.WCFConfigBasedService => new WCFProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.VBClassLibrary => new VisualBasicProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.VBWebApi => new VisualBasicProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.VBWebForms => new VisualBasicProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.VBNetMvc => new VisualBasicProjectRewriter(analyzerResult, projectConfiguration),
                _ => new ProjectRewriter(analyzerResult, projectConfiguration)
            };
            return projectRewriter;
        }
        
        public ProjectRewriter GetInstance(IDEProjectResult ideProjectResult, ProjectConfiguration projectConfiguration)
        {
            var projectType = projectConfiguration.ProjectType;
            var projectRewriter = projectType switch
            {
                ProjectType.WCFCodeBasedService => new WCFProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.WCFConfigBasedService => new WCFProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.VBClassLibrary => new VisualBasicProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.VBWebApi => new VisualBasicProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.VBWebForms => new VisualBasicProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.VBNetMvc => new VisualBasicProjectRewriter(ideProjectResult, projectConfiguration),
                _ => new ProjectRewriter(ideProjectResult, projectConfiguration)
            };
            return projectRewriter;
        }
    }
}
