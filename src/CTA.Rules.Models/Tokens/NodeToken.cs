using System.Linq;
using System.Collections.Generic;
using CTA.Rules.Config;
using TextSpan = Codelyzer.Analysis.Model.TextSpan;

namespace CTA.Rules.Models.Tokens
{
    public class NodeToken
    {
        public NodeToken()
        {
            ExpressionActions = new List<ExpressionAction>();
            MemberAccessActions = new List<MemberAccessAction>();
            PackageActions = new List<PackageAction>();
            ProjectLevelActions = new List<ProjectLevelAction>();
            ProjectFileActions = new List<ProjectLevelAction>();
            ProjectTypeActions = new List<ProjectLevelAction>();
            TargetCPU = new List<string>();
            TextChanges = new List<TextChange>();
        }
        public string Key { get; set; }
        public string TrimmedKey => !string.IsNullOrEmpty(Key) ? Utils.EscapeAllWhitespace(Key) : string.Empty;
        public string Namespace { get; set; }
        public string Type { get; set; }
        public string FullKey { get; set; }
        public TextSpan TextSpan { get; set; }
        public string Description { get; set; }
        public IList<TextChange> TextChanges { get; set; }
        public List<string> TargetCPU { get; set; }

        public List<MemberAccessAction> MemberAccessActions { get; set; }
        public List<ExpressionAction> ExpressionActions { get; set; }
        public List<PackageAction> PackageActions { get; set; }
        public List<ProjectLevelAction> ProjectLevelActions { get; set; }
        public List<ProjectLevelAction> ProjectFileActions { get; set; }
        public List<ProjectLevelAction> ProjectTypeActions { get; set; }

        public virtual NodeToken Clone()
        {
            NodeToken cloned = (NodeToken)this.MemberwiseClone();
            cloned.TextChanges = cloned.TextChanges?.Select(textChange => textChange.Clone()).ToList();
            cloned.TargetCPU = cloned.TargetCPU?.ToList(); 
            cloned.MemberAccessActions = cloned.MemberAccessActions.Select(action => action.Clone<MemberAccessAction>()).ToList();
            cloned.ExpressionActions = cloned.ExpressionActions.Select(action => action.Clone<ExpressionAction>()).ToList();
            cloned.PackageActions = cloned.PackageActions.Select(action => action.Clone()).ToList();
            cloned.ProjectLevelActions = cloned.ProjectLevelActions.Select(action => action.Clone<ProjectLevelAction>()).ToList();
            cloned.ProjectFileActions = cloned.ProjectFileActions.Select(action => action.Clone<ProjectLevelAction>()).ToList();
            cloned.ProjectTypeActions = cloned.ProjectTypeActions.Select(action => action.Clone<ProjectLevelAction>()).ToList();
            return cloned;
        }

        public virtual List<GenericAction> AllActions
        {
            get
            {
                var allActions = new List<GenericAction>();
                allActions.AddRange(MemberAccessActions);
                allActions.AddRange(ExpressionActions);
                allActions.AddRange(MemberAccessActions);
                return allActions;
            }
        }

    }
}
