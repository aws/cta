using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTA.Rules.Config;
using CTA.Rules.Models.VisualBasic;

namespace CTA.Rules.Models
{
    public class ProjectActions
    {
        public ProjectActions()
        {
            FileActions = new BlockingCollection<FileActions>();
            PackageActions = new BlockingCollection<PackageAction>();
            ProjectReferenceActions = new BlockingCollection<string>();
            ProjectLevelActions = new List<ProjectLevelAction>();
        }
        public BlockingCollection<FileActions> FileActions { get; set; }
        public BlockingCollection<PackageAction> PackageActions { get; set; }
        public BlockingCollection<string> ProjectReferenceActions { get; set; }
        public List<ProjectLevelAction> ProjectLevelActions { get; set; }
        public CsharpRootNodes CsharpProjectRules { get; set; }
        public VisualBasicRootNodes VbProjectRules { get; set; }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            var fileActions = FileActions.ToList().OrderBy(f => f.FilePath);

            foreach (var fileAction in fileActions)
            {
                var actions = new List<string>();

                StringBuilder fileChanges = new StringBuilder();
                foreach (var action in fileAction.AllActions)
                {
                    actions.Add(action.Description);
                }

                if (actions.Count > 0)
                {
                    str.AppendLine(fileAction.FilePath);
                    str.AppendLine(string.Join(Environment.NewLine, actions));
                }
            }

            str.AppendLine(Constants.Project);
            var projectActions = new List<string>();
            foreach (var action in ProjectLevelActions)
            {
                projectActions.Add(action.Description);
            }

            str.AppendLine(string.Join(Environment.NewLine, projectActions));

            return str.ToString();
        }

        public string ToSummaryString()
        {
            StringBuilder str = new StringBuilder();

            var fileActions = FileActions.ToList().OrderBy(f => f.FilePath);

            foreach (var fileAction in fileActions)
            {
                var actions = new List<string>();

                foreach (var action in fileAction.AllActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }

                if (actions.Count > 0)
                {
                    str.AppendLine(fileAction.FilePath);
                    str.AppendLine(string.Join(Environment.NewLine, actions));
                }
            }

            str.AppendLine(Constants.Project);
            var projectActions = new List<string>();
            foreach (var action in ProjectLevelActions)
            {
                projectActions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
            }

            str.AppendLine(string.Join(Environment.NewLine, projectActions));

            return str.ToString();
        }
    }
}
