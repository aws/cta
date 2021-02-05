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
        private string _rulesFile;
        private string _targetFramework;
        private string _overrideFile;
        private string _assembliesDir;
        private IEnumerable<Reference> _projectReferences;

        /// <summary>
        /// Initializes a new RulesFileLoader
        /// </summary>
        /// <param name="projectReferences">References in the project to filter the rules by</param>
        /// <param name="overrideFile">Path to rules file containing override rules. The override rules will be added to the built in rules, overriding any matching existing rules</param>
        /// <param name="assembliesDir">Directory containing assemblies containing additional actions</param>
        public RulesFileLoader(IEnumerable<Reference> projectReferences, string rulesFile, List<string> targetFramework, string overrideFile = "", string assembliesDir = "")
        {
            _rulesFile = rulesFile;
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
                if (!string.IsNullOrEmpty(_rulesFile) && Directory.Exists(_rulesFile))
                {
                    rulesFile = LoadNamespaceFile(_rulesFile);
                }
                return rulesFile;
            });
            mainNamespaceFileTasks.Start();

            var mainFileTask = new Task<Rootobject>(() =>
            {
                Rootobject rulesFile = new Rootobject();
                if (!string.IsNullOrEmpty(_rulesFile) && Directory.Exists(_rulesFile))
                {
                    rulesFile = LoadFile(_rulesFile);
                    if (rulesFile.NameSpaces != null)
                    {
                        rulesFile.NameSpaces = rulesFile.NameSpaces.Where(n => _projectReferences.Contains(new Reference() { Assembly = n.Assembly, Namespace = n.@namespace }) || (n.Assembly == Constants.Project && n.@namespace == Constants.Project)).ToList();
                    }
                }
                return rulesFile;
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
                Rootobject rulesFile = new Rootobject();
                if (!string.IsNullOrEmpty(_overrideFile) && Directory.Exists(_overrideFile))
                {
                    rulesFile = LoadFile(_overrideFile);
                    if (rulesFile.NameSpaces != null)
                    {
                        rulesFile.NameSpaces = rulesFile.NameSpaces.Where(n => _projectReferences.Contains(new Reference() { Assembly = n.Assembly, Namespace = n.@namespace }) || (n.Assembly == Constants.Project && n.@namespace == Constants.Project)).ToList();
                    }
                }
                return rulesFile;
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

            var ruleFiles = Directory.EnumerateFiles(pathToLoad, "*.json", SearchOption.AllDirectories);
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

        private Rootobject LoadFile(string pathToLoad)
        {
            Rootobject r = new Rootobject();
            var ruleFiles = Directory.EnumerateFiles(pathToLoad, "*.json", SearchOption.AllDirectories);
            foreach (var ruleFile in ruleFiles)
            {
                try
                {
                    var content = File.ReadAllText(ruleFile);

                    var currentNode = JsonConvert.DeserializeObject<Rootobject>(content);
                    r.NameSpaces.AddRange(currentNode.NameSpaces);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError("Error parsing file: {0}{1} Exception:{2}", ruleFile, Environment.NewLine, ex.Message);
                }
            }
            return r;
        }

    }
}
