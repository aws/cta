using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis.Model;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;

namespace CTA.Rules.RuleFiles
{
    /// <summary>
    /// Loads the rules file(s)
    /// </summary>
    public class RulesFileLoader
    {
        private readonly string _rulesFilesDir;
        private readonly string _targetFramework;
        private readonly string _overrideFile;
        private readonly string _assembliesDir;
        private readonly IEnumerable<Reference> _projectReferences;

        /// <summary>
        /// Initializes a new RulesFileLoader
        /// </summary>
        /// <param name="projectReferences">References in the project to filter the rules by</param>
        /// <param name="rulesFilesDir">Directory containing rules file(s) to use</param>
        /// <param name="targetFramework">Target framework to port to</param>
        /// <param name="overrideFile">Path to rules file containing override rules. The override rules will be added to the built in rules, overriding any matching existing rules</param>
        /// <param name="assembliesDir">Directory containing assemblies containing additional actions</param>
        public RulesFileLoader(IEnumerable<Reference> projectReferences, string rulesFilesDir, List<string> targetFramework, string overrideFile = "", string assembliesDir = "")
        {
            _rulesFilesDir = rulesFilesDir;
            try
            {
                _targetFramework = targetFramework.First();
                if (targetFramework.Count > 1)
                {
                    LogHelper.LogDebug("Please specify one target version. Multiple target versions is not supported");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Please specify one target version. Multiple target versions is not supported");
            }

            _overrideFile = overrideFile;
            _assembliesDir = assembliesDir;
            _projectReferences = projectReferences;
        }

        /// <summary>
        /// Loads rules from the main rules file and override file
        /// </summary>
        /// <returns>A RootNodes object containing all the rules after being merged</returns>
        public RootNodes Load()
        {
            var mainNamespaceFileTasks = new Task<NamespaceRecommendations>(() =>
            {
                NamespaceRecommendations rulesFile = new NamespaceRecommendations();
                if (!string.IsNullOrEmpty(_rulesFilesDir) && Directory.Exists(_rulesFilesDir))
                {
                    rulesFile = LoadNamespaceFile(_rulesFilesDir);
                }
                return rulesFile;
            });
            mainNamespaceFileTasks.Start();

            var mainFileTask = new Task<Rootobject>(() =>
            {
                Rootobject rules = new Rootobject();
                if (!string.IsNullOrEmpty(_rulesFilesDir) && Directory.Exists(_rulesFilesDir))
                {
                    rules = LoadRulesFiles(_rulesFilesDir);
                    if (rules.NameSpaces != null)
                    {
                        rules.NameSpaces = rules.NameSpaces.Where(n => _projectReferences.Contains(new Reference() { Assembly = n.Assembly, Namespace = n.@namespace }) || (n.Assembly == Constants.Project)).ToList();
                    }
                }
                return rules;
            });
            mainFileTask.Start();

            var overrideNamespaceFileTasks = new Task<NamespaceRecommendations>(() =>
            {
                NamespaceRecommendations rulesFile = new NamespaceRecommendations();
                if (!string.IsNullOrEmpty(_overrideFile) && Directory.Exists(_overrideFile))
                {
                    rulesFile = LoadNamespaceFile(_overrideFile);
                }
                return rulesFile;
            });
            overrideNamespaceFileTasks.Start();

            var overrideTask = new Task<Rootobject>(() =>
            {
                Rootobject rules = new Rootobject();
                if (!string.IsNullOrEmpty(_overrideFile) && Directory.Exists(_overrideFile))
                {
                    rules = LoadRulesFiles(_overrideFile);
                    if (rules.NameSpaces != null)
                    {
                        rules.NameSpaces = rules.NameSpaces.Where(n => _projectReferences.Contains(new Reference() { Assembly = n.Assembly, Namespace = n.@namespace }) || (n.Assembly == Constants.Project && n.@namespace == Constants.Project)).ToList();
                    }
                }
                return rules;
            });
            overrideTask.Start();

            Task.WaitAll(mainNamespaceFileTasks, overrideNamespaceFileTasks, mainFileTask, overrideTask);

            RulesFileParser rulesFileParser = new RulesFileParser(mainNamespaceFileTasks.Result,
                overrideNamespaceFileTasks.Result,
                mainFileTask.Result,
                overrideTask.Result,
                _assembliesDir,
                _targetFramework
                );
            var rootNodes = rulesFileParser.Process();

            return rootNodes;
        }


        private NamespaceRecommendations LoadNamespaceFile(string pathToLoad)
        {
            NamespaceRecommendations nr = new NamespaceRecommendations();

            var ruleFiles = Directory.EnumerateFiles(pathToLoad, "*.json", SearchOption.AllDirectories).Where(r => _projectReferences.Select(p => p.Namespace?.ToLower()).Contains(Path.GetFileNameWithoutExtension(r))).ToList();
            foreach (var ruleFile in ruleFiles)
            {
                try
                {
                    var content = File.ReadAllText(ruleFile);

                    var currentNode = JsonConvert.DeserializeObject<Namespaces>(content);
                    if (currentNode != null && !string.IsNullOrEmpty(currentNode.Name))
                    {
                        nr.NameSpaces.Add(currentNode);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError("Error parsing file: {0}{1} Exception:{2}", ruleFile, Environment.NewLine, ex.Message);
                    return nr;
                }

            }
            return nr;
        }

        private Rootobject LoadRulesFiles(string ruleFilesDir)
        {
            Rootobject r = new Rootobject();
            var rulesFiles = Directory.EnumerateFiles(ruleFilesDir, "*.json", SearchOption.AllDirectories);
            foreach (var rulesFile in rulesFiles)
            {
                try
                {
                    var content = File.ReadAllText(rulesFile);

                    var currentNode = JsonConvert.DeserializeObject<Rootobject>(content);
                    r.NameSpaces.AddRange(currentNode.NameSpaces);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError("Error parsing file: {0}{1} Exception:{2}", rulesFile, Environment.NewLine, ex.Message);
                }
            }
            return r;
        }

    }
}
