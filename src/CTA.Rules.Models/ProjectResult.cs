using CTA.Rules.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Models
{
    public class ProjectResult
    {
        public ProjectResult()
        {
            ExecutedActions = new Dictionary<string, List<GenericActionExecution>>();
        }
        public string ProjectFile { get; set; }
        public ProjectActions ProjectActions { get; set; }
        public Dictionary<string, List<GenericActionExecution>> ExecutedActions { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var fileName in ExecutedActions.Keys)
            {
                stringBuilder.AppendLine("---------------------------");
                stringBuilder.AppendLine($"Executed Actions for file: {fileName}");
                stringBuilder.AppendLine("---------------------------");
                foreach (var genericAction in ExecutedActions[fileName])
                {
                    var actionKey = string.IsNullOrEmpty(genericAction.Key) ? "N/A" : genericAction.Key;

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
