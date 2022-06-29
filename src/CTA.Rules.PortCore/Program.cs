﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.Extensions.Logging;

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


                if (string.IsNullOrEmpty(cli.RulesDir))
                {
                    //TODO : Change the hard coded path to a constant
                    cli.RulesDir = Config.Constants.RulesDefaultPath;
                }

                if (cli.CreateNew)
                {
                    cli.FilePath = Utils.CopySolutionToTemp(cli.FilePath);
                }

                var projectFiles = Utils.GetProjectPaths(cli.FilePath);

                var packageReferences = new Dictionary<string, Tuple<string, string>>
                {
                    { "Microsoft.EntityFrameworkCore", new Tuple<string, string>("0.0.0", "*") }
                };

                List<PortCoreConfiguration> configs = new List<PortCoreConfiguration>();
                foreach (var proj in projectFiles)
                {
                    PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
                    {
                        SolutionPath = cli.FilePath,
                        ProjectPath = proj,
                        RulesDir = cli.RulesDir,
                        IsMockRun = cli.IsMockRun,
                        UseDefaultRules = cli.DefaultRules,
                        PackageReferences = packageReferences,
                        PortCode = true,
                        PortProject = true,
                        TargetVersions = new List<string> { cli.Version },
                        SourceVersions = new List<string> { Constants.DefaultNetFrameworkVersion }
                    };

                    configs.Add(projectConfiguration);
                }

                //Solution Rewriter:
                SolutionPort solutionPort = new SolutionPort(cli.FilePath, configs);
                var s = solutionPort.AnalysisRun();
                foreach (var k in s.ProjectResults)
                {
                    Console.WriteLine(k.ProjectFile);
                    Console.WriteLine(k.ProjectActions.ToString());
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
