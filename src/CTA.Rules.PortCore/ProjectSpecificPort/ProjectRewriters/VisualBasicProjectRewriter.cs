using System.Collections.Generic;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Models;
using CTA.Rules.Update;

namespace CTA.Rules.PortCore
{
    public class VisualBasicProjectRewriter : ProjectRewriter
    {
        public readonly ProjectType _projectType; 

        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public VisualBasicProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
            : base(analyzerResult, projectConfiguration)
        {
            _projectType = projectConfiguration.ProjectType;
        }

        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="projectResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public VisualBasicProjectRewriter(IDEProjectResult projectResult, ProjectConfiguration projectConfiguration)
           : base(projectResult, projectConfiguration)
        {
            _projectType = projectConfiguration.ProjectType;
        }

        /// <summary>
        /// Initializes the ProjectRewriter then runs it
        /// </summary>
        public override ProjectResult Run()
        {
            if (isExludedFromPorting(_projectType))
                return _projectResult;
               
            var projectResult = Initialize();
            return Run(projectResult.ProjectActions);
        }

        /// <summary>
        /// Runs the project rewriter using a previously initialized analysis
        /// </summary>
        /// <param name="projectActions"></param>
        public override ProjectResult Run(ProjectActions projectActions)
        {
            if (isExludedFromPorting(_projectType))
                return _projectResult;

            base.Run(projectActions);
            return _projectResult;
        }

        /// <summary>
        /// Runs the project rewriter using an incremental analysis
        /// </summary>
        /// <param name="updatedFiles"></param>
        /// <param name="projectRules"></param>
        public override List<IDEFileActions> RunIncremental(List<string> updatedFiles, CsharpRootNodes projectRules)
        {
            List<IDEFileActions> ideFileActions = new List<IDEFileActions>();
            if(!isExludedFromPorting(_projectType))
                ideFileActions =  base.RunIncremental(updatedFiles, projectRules);
            return ideFileActions;
        }


        private bool isExludedFromPorting(ProjectType projectType)
        {
            return projectType == ProjectType.VBWebForms || projectType == ProjectType.VBNetMvc;
        }
    }
}
