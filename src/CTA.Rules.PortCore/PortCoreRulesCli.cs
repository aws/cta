using System;
using System.Collections.Generic;
using CommandLine;

namespace CTA.Rules.PortCore
{
    public class PortCoreOptions
    {
        [Option('p', "project-path", Required = false, HelpText = "Project file path.")]
        public string ProjectPath { get; set; }

        [Option('s', "solution-path", Required = false, HelpText = "Solution file path.")]
        public string SolutionPath { get; set; }

        [Option('r', "rules-dir", Required = false, HelpText = "Directory containing rules json input file(s)")]
        public string RulesDir { get; set; }

        [Option('d', "use-builtin-rules", Required = false, HelpText = "Use default rule files")]
        public string DefaultRules { get; set; }

        [Option('a', "assemblies-dir", Required = false, HelpText = "Action Assemblies Dir")]
        public string AssembliesDir { get; set; }

        [Option('v', "version", Required = false, HelpText = "Version of net core to port to (netcoreapp3.1 or net5.0)")]
        public string Version { get; set; }

        [Option('m', "mock-run", Required = false, HelpText = "Mock run to generate output only (no changes will be made)")]
        public string IsMockRun { get; set; }

        [Option('c', "create-new", Required = false, HelpText = "Create a new folder for ported solution")]
        public string CreateNew { get; set; }

    }

    public class PortCoreRulesCli
    {
        public string FilePath;
        public string RulesDir;
        public string AssembliesDir;
        public bool IsMockRun;
        public bool DefaultRules;
        public string Version;
        public bool CreateNew;

        public void HandleCommand(String[] args)
        {
            Parser.Default.ParseArguments<PortCoreOptions>(args)
                .WithNotParsed(HandleParseError)
                .WithParsed<PortCoreOptions>(o =>
                {
                    if (!string.IsNullOrEmpty(o.ProjectPath))
                    {
                        FilePath = o.ProjectPath;
                    }
                    else
                    {
                        FilePath = o.SolutionPath;
                    }

                    RulesDir = o.RulesDir;
                    AssembliesDir = o.AssembliesDir;

                    if (!string.IsNullOrEmpty(o.DefaultRules) && o.DefaultRules.ToLower() == "true")
                    {
                        DefaultRules = true;
                    }
                    if (!string.IsNullOrEmpty(o.Version))
                    {
                        Version = o.Version;
                    }
                    else
                    {
                        Version = "netcoreapp3.1";
                    }
                    if (!string.IsNullOrEmpty(o.IsMockRun) && o.IsMockRun.ToLower() == "true")
                    {
                        IsMockRun = true;
                    }
                    if (!string.IsNullOrEmpty(o.CreateNew) && o.CreateNew.ToLower() == "true")
                    {
                        CreateNew = true;
                    }
                });
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Environment.Exit(-1);
        }
    }


}
