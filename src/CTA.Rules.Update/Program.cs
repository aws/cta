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


                string solutionDir = Directory.GetParent(cli.FilePath).FullName;
                var projectFiles = Directory.EnumerateFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);

                List<ProjectConfiguration> configs = new List<ProjectConfiguration>();
                foreach (var proj in projectFiles)
                {
                    ProjectConfiguration projectConfiguration = new ProjectConfiguration()
                    {
                        ProjectPath = proj,
                        RulesDir = cli.RulesDir,
                        IsMockRun = cli.IsMockRun,
                        TargetVersions = new List<string> { "net5.0" }
                    };

                    configs.Add(projectConfiguration);
                }

                //Solution Rewriter:
                SolutionRewriter solutionRewriter = new SolutionRewriter(cli.FilePath, configs);
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
