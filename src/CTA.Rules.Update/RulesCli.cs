using System;
using System.Collections.Generic;
using Codelyzer.Analysis;
using CommandLine;

namespace CTA.Rules.Update
{
    public class Options
    {
        [Option('p', "project-path", Required = true, HelpText = "Project file path.")]
        public string ProjectPath { get; set; }

        [Option('s', "solution-path", Required = false, HelpText = "Solution file path.")]
        public string SolutionPath { get; set; }

        [Option('r', "rules-input-infile", Required = false, HelpText = "Rules json input file")]
        public string RulesInputFile { get; set; }

        [Option('a', "assemblies-dir", Required = false, HelpText = "Action Assemblies Dir")]
        public string AssembliesDir { get; set; }

        [Option('m', "mock-run", Required = false, HelpText = "Mock run to generate output only (no changes will be made)")]
        public string IsMockRun { get; set; }
    }

    public class RulesCli
    {
        public bool Project;
        public string FilePath;
        public string RulesPath;
        public string AssembliesDir;
        public bool IsMockRun;
        public AnalyzerConfiguration Configuration;

        public void HandleCommand(String[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(HandleParseError)
                .WithParsed<Options>(o =>
                {
                    if (!string.IsNullOrEmpty(o.ProjectPath))
                    {
                        Project = true;
                        FilePath = o.ProjectPath;
                    }

                    RulesPath = o.RulesInputFile;
                    AssembliesDir = o.AssembliesDir;

                    if (!string.IsNullOrEmpty(o.IsMockRun) && o.IsMockRun.ToLower() == "true")
                    {
                        IsMockRun = true;
                    }
                });
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Environment.Exit(-1);
        }
    }


}