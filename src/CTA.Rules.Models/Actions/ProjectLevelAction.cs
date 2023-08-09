using System;
using System.Collections.Generic;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;

namespace CTA.Rules.Models
{
    public class ProjectLevelAction : GenericAction
    {
        public Func<string, ProjectType, string> ProjectLevelActionFunc { get; set; }
        public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, List<string>, string> ProjectFileActionFunc { get; set; }
        public Func<ProjectType, ProjectConfiguration, ProjectResult, AnalyzerResult, string> ProjectTypeActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (ProjectLevelAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                &&
                (
                    (action.ProjectLevelActionFunc != null && ProjectLevelActionFunc != null && action.ProjectLevelActionFunc.Method.Name == ProjectLevelActionFunc.Method.Name)
                    ||
                    (action.ProjectFileActionFunc != null && ProjectFileActionFunc != null && action.ProjectFileActionFunc.Method.Name == ProjectFileActionFunc.Method.Name)
                    ||
                    (action.ProjectTypeActionFunc != null && ProjectTypeActionFunc != null && action.ProjectTypeActionFunc.Method.Name == ProjectTypeActionFunc.Method.Name)
                );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, ProjectLevelActionFunc?.Method.Name, ProjectFileActionFunc?.Method.Name, ProjectTypeActionFunc?.Method.Name);
        }
    }
}
