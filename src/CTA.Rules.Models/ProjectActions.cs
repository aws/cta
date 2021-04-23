using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTA.Rules.Config;

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
        public RootNodes ProjectRules { get; set; }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            var fileActions = FileActions.ToList().OrderBy(f => f.FilePath);

            foreach (var fileAction in fileActions)
            {
                var actions = new List<string>();

                StringBuilder fileChanges = new StringBuilder();
                foreach (var action in fileAction.AttributeActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.ClassDeclarationActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.ElementAccessActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.IdentifierNameActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.InvocationExpressionActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.MemberAccessActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.MethodDeclarationActions)
                {
                    actions.Add(action.Description);
                }
                foreach (var action in fileAction.Usingactions)
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

                StringBuilder fileChanges = new StringBuilder();
                foreach (var action in fileAction.AttributeActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.ClassDeclarationActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.ElementAccessActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.IdentifierNameActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.InvocationExpressionActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.MemberAccessActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.MethodDeclarationActions)
                {
                    actions.Add(string.Concat(action.Type, ":", action.Name, ":", action.Key));
                }
                foreach (var action in fileAction.Usingactions)
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
