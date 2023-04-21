using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.Extensions.Logging;

namespace CTA.Rules.Update
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                RulesCli cli = new RulesCli();
                cli.HandleCommand(args);
                Console.WriteLine(cli.Project + " -- " + cli.FilePath);

                /* 1. Logger object */
                var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());

                LogHelper.Logger = loggerFactory.CreateLogger("Translator");

                var projectFiles = Utils.GetProjectPaths(cli.FilePath);

                List<ProjectConfiguration> configs = new List<ProjectConfiguration>();
                foreach (var proj in projectFiles)
                {
                    ProjectConfiguration projectConfiguration = new ProjectConfiguration()
                    {
                        SolutionPath = cli.FilePath,
                        ProjectPath = proj,
                        RulesDir = cli.RulesDir,
                        IsMockRun = cli.IsMockRun,
                        TargetVersions = new List<string> { SupportedFrameworks.Net5 },
                        SourceVersions = new List<string> { Constants.DefaultNetFrameworkVersion }
                    };

                    configs.Add(projectConfiguration);
                }

                //Solution Rewriter:
                SolutionRewriter solutionRewriter = new SolutionRewriter(cli.FilePath, configs, syntaxOnly: cli.IsSyntaxOnlyBuild);
                var s = solutionRewriter.AnalysisRun();
                foreach (var k in s.ProjectResults)
                {
                    Console.WriteLine(k.ProjectFile);
                    Console.WriteLine(k.ProjectActions.ToString());
                }

                solutionRewriter.Run(s.ProjectResults.ToDictionary(p => p.ProjectFile, p => p.ProjectActions));
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Error while running solution rewriter: {0}", ex.Message);
            }
        }
    }
}
