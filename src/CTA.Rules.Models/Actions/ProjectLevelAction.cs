using System;
using System.Collections.Generic;

namespace CTA.Rules.Models
{
    public class ProjectLevelAction : GenericAction
    {
        public Func<string, ProjectType, string> ProjectLevelActionFunc { get; set; }
        public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, List<string>, string> ProjectFileActionFunc { get; set; }

        public new ProjectLevelAction Clone() => (ProjectLevelAction)this.MemberwiseClone();
        public override bool Equals(object obj)
        {
            var action = (ProjectLevelAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                &&
                (
                (action.ProjectLevelActionFunc != null && this.ProjectLevelActionFunc != null && action.ProjectLevelActionFunc.Method.Name == this.ProjectLevelActionFunc.Method.Name)
                ||
                (action.ProjectFileActionFunc != null && this.ProjectFileActionFunc != null && action.ProjectFileActionFunc.Method.Name == this.ProjectFileActionFunc.Method.Name)
                );
        }

        public override int GetHashCode()
        {
            return 3 * Value.GetHashCode()
                + 5 * (ProjectLevelActionFunc != null ? ProjectLevelActionFunc.Method.Name.GetHashCode() : 0)
                + 7 * (ProjectFileActionFunc != null ? ProjectFileActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}
