using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Newtonsoft.Json;

namespace CTA.Rules.RuleFiles
{
    /// <summary>
    /// Export tool to allow exporting the rules file to a flat format to be consumed by porting assistant recommendations
    /// </summary>
    public class RulesFileExport
    {
        private readonly Rootobject _rootObject;
        private readonly string _rulesPath;
        private readonly List<TargetFramework> targetFrameworks;

        /// <summary>
        /// Initialize a new instance of RulesFileExport
        /// </summary>
        /// <param name="rulesPath">Path to rules JSON file</param>
        public RulesFileExport(string rulesPath)
        {
            _rulesPath = rulesPath;
            string rulesFileContent = File.ReadAllText(rulesPath);

            _rootObject = JsonConvert.DeserializeObject<Rootobject>(rulesFileContent);

            targetFrameworks = SupportedFrameworks.GetSupportedFrameworksList().Select(framework => new TargetFramework() { Name = framework, TargetCPU = SupportedCPUs.GetSupportedCPUsList() }).ToList();
        }

        /// <summary>
        /// Exports the files
        /// </summary>
        public void Run()
        {
            NamespaceRecommendations recommendations = new NamespaceRecommendations();

            foreach (var @namespace in _rootObject.NameSpaces)
            {
                if (@namespace.Actions != null && @namespace.Actions.Count > 0)
                {
                    AddToFlatFile(@namespace, recommendations);
                }
                foreach (var @class in @namespace.Classes)
                {
                    if (@class.Actions != null && @class.Actions.Count > 0)
                    {
                        AddToFlatFile(@class, @namespace, recommendations);
                    }
                    foreach (var attribute in @class.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            AddToFlatFile(attribute, @namespace, @class, recommendations);
                        }
                    }

                    foreach (var method in @class.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            AddToFlatFile(method, @namespace, @class, recommendations);
                        }
                    }
                }
            }

            SaveFiles(recommendations);
        }
        private void SaveFiles(NamespaceRecommendations recommendations)
        {
            string dir = Directory.GetParent(_rulesPath).FullName;

            foreach (var n in recommendations.NameSpaces)
            {
                string fileName = Path.Combine(dir, string.Concat(n.Name.ToLower(), ".json"));
                var result = JsonConvert.SerializeObject(n, Formatting.Indented);
                File.WriteAllText(fileName, result);
            }
        }
        private void AddToFlatFile(Namespace @namespace, NamespaceRecommendations recommendations)
        {
            var currentNamespace = EnsureNamespace(@namespace, recommendations);
            if (@namespace.Actions != null && @namespace.Actions.Count > 0)
            {

                RecommendedActions recommendation = new RecommendedActions()
                {
                    Source = "Amazon",
                    Preferred = "Yes",
                    TargetFrameworks = targetFrameworks,
                    Description = GetDescription(@namespace.Actions),
                    Actions = @namespace.Actions
                };

                Recommendations r = new Recommendations()
                {
                    Type = "Namespace",
                    Name = @namespace.@namespace,
                    KeyType = "Name",
                    Value = @namespace.@namespace,
                    ContainingType = string.Empty,

                    RecommendedActions = new List<RecommendedActions>() { recommendation }
                };

                currentNamespace.Recommendations.Add(r);
            }
        }
        private void AddToFlatFile(@Class @class, Namespace @namespace, NamespaceRecommendations recommendations)
        {
            var currentNamespace = EnsureNamespace(@namespace, recommendations);
            RecommendedActions recommendation = new RecommendedActions()
            {
                Source = "Amazon",
                Preferred = "Yes",
                TargetFrameworks = targetFrameworks,
                Description = GetDescription(@class.Actions),
                Actions = @class.Actions
            };

            Recommendations r = new Recommendations()
            {
                Type = "Class",
                KeyType = @class.KeyType,
                Name = @class.Key,
                Value = @class.FullKey,
                ContainingType = string.Empty,
                RecommendedActions = new List<RecommendedActions>() { recommendation }
            };
            currentNamespace.Recommendations.Add(r);

        }
        private void AddToFlatFile(Method method, Namespace @namespace, @Class @class, NamespaceRecommendations recommendations)
        {
            var currentNamespace = EnsureNamespace(@namespace, recommendations);

            RecommendedActions recommendation = new RecommendedActions()
            {
                Source = "Amazon",
                Preferred = "Yes",
                TargetFrameworks = targetFrameworks,
                Description = GetDescription(method.Actions),
                Actions = method.Actions
            };

            Recommendations r = new Recommendations()
            {
                Type = "Method",
                Name = method.Key,
                KeyType = "Name",
                Value = method.FullKey,
                ContainingType = @class.Key,
                RecommendedActions = new List<RecommendedActions>() { recommendation }
            };

            currentNamespace.Recommendations.Add(r);
        }
        private void AddToFlatFile(Attribute attribute, Namespace @namespace, @Class @class, NamespaceRecommendations recommendations)
        {
            var currentNamespace = EnsureNamespace(@namespace, recommendations);

            RecommendedActions recommendation = new RecommendedActions()
            {
                Source = "Amazon",
                Preferred = "Yes",
                TargetFrameworks = targetFrameworks,
                Description = GetDescription(attribute.Actions),
                Actions = attribute.Actions
            };

            Recommendations r = new Recommendations()
            {
                Type = "Attribute",
                Name = attribute.Key,
                KeyType = "Name",
                Value = attribute.FullKey,
                ContainingType = @class.Key,
                RecommendedActions = new List<RecommendedActions>() { recommendation }
            };
            currentNamespace.Recommendations.Add(r);
        }
        private Namespaces EnsureNamespace(Namespace @namespace, NamespaceRecommendations recommendations)
        {
            var currentNamespaceName = string.Empty;

            var currentNamespace = recommendations.NameSpaces.Where(n => n.Name == @namespace.@namespace).FirstOrDefault();

            if (currentNamespace == null)
            {
                currentNamespace = new Namespaces()
                {
                    Name = @namespace.@namespace,
                    Packages = new List<Packages>() { new Packages() { Name = @namespace.Assembly, Type = @namespace.Type } },
                    Version = "1.0.0"
                };
                recommendations.NameSpaces.Add(currentNamespace);
            }

            return currentNamespace;
        }
        private string GetDescription(List<Action> actions)
        {
            if (actions.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder str = new StringBuilder();
            actions.ForEach((a) => { str.AppendLine(a.Description); });
            return str.ToString();
        }


    }


}
