using System.Collections.Generic;
using System.Text;
using CTA.Rules.Config;

namespace CTA.Rules.Models
{
    public class ProjectResult
    {
        public List<PackageAction> UpgradePackages { get; set; }
        public List<PackageAction> ActionPackages { get; set; }
        public List<string> TargetVersions { get; set; }

        public ProjectResult()
        {
            ExecutedActions = new Dictionary<string, List<GenericActionExecution>>();
            UpgradePackages = new List<PackageAction>();
            ActionPackages = new List<PackageAction>();
        }
        public string ProjectFile { get; set; }
        public ProjectActions ProjectActions { get; set; }
        public Dictionary<string, List<GenericActionExecution>> ExecutedActions { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("---------------------------");
            stringBuilder.AppendLine("Target Versions:");
            stringBuilder.AppendLine("---------------------------");
            foreach (var version in TargetVersions)
            {
                stringBuilder.AppendLine(version);
            }


            stringBuilder.AppendLine("---------------------------");
            stringBuilder.AppendLine("Upgrade packages:");
            stringBuilder.AppendLine("---------------------------");
            foreach (var package in UpgradePackages)
            {
                stringBuilder.AppendLine($"{package.Name},{package.OriginalVersion}->{package.Version}");
            }

            stringBuilder.AppendLine("---------------------------");
            stringBuilder.AppendLine("Action packages:");
            stringBuilder.AppendLine("---------------------------");
            foreach (var package in ActionPackages)
            {
                stringBuilder.AppendLine($"{package.Name},{package.Version}");
            }

            foreach (var fileName in ExecutedActions.Keys)
            {
                stringBuilder.AppendLine("---------------------------");
                stringBuilder.AppendLine($"Executed Actions for file: {fileName}");
                stringBuilder.AppendLine("---------------------------");
                foreach (var genericAction in ExecutedActions[fileName])
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine($"Action Type: {genericAction.Type}");
                    stringBuilder.AppendLine($"Action Name: {genericAction.Name}");
                    stringBuilder.AppendLine($"Action Value: {Utils.EscapeAllWhitespace(genericAction.Value)}");
                    stringBuilder.AppendLine($"Times Run: {genericAction.TimesRun}");
                    stringBuilder.AppendLine($"Invalid Executions: {genericAction.InvalidExecutions}");
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }
    }
}
