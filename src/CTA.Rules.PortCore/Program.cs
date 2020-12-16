using CTA.Rules.Config;
using CTA.Rules.Models;
using Codelyzer.Analysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace CTA.Rules.PortCore
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                PortCoreRulesCli cli = new PortCoreRulesCli();
                cli.HandleCommand(args);
                Console.WriteLine(cli.FilePath);

                /* 1. Logger object */
                var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());

                LogHelper.Logger = loggerFactory.CreateLogger("Translator");


                if (string.IsNullOrEmpty(cli.RulesPath))
                {
                    //TODO : Change the hard coded path to a constant
                    cli.RulesPath = Config.Constants.RulesDefaultPath;
                }

                string solutionDir = Directory.GetParent(cli.FilePath).FullName;
                var projectFiles = Directory.EnumerateFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);

                List<PortCoreConfiguration> configs = new List<PortCoreConfiguration>();
                foreach (var proj in projectFiles)
                {
                    PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
                    {
                        ProjectPath = proj,
                        RulesPath = cli.RulesPath,
                        IsMockRun = cli.IsMockRun,
                        UseDefaultRules = cli.DefaultRules,
                        TargetVersions = new List<string> { cli.Version }
                    };

                    configs.Add(projectConfiguration);
                }

                //Solution Rewriter:
                SolutionPort solutionPort = new SolutionPort(cli.FilePath, configs);
                var s = solutionPort.AnalysisRun();
                foreach (var k in s.Keys)
                {
                    Console.WriteLine(k);
                    Console.WriteLine(s[k].ToString());
                }
                var portSolutionResult = solutionPort.Run();
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Error while running solution rewriter: {0}", ex.Message);
            }
        }
    }

}
