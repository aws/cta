using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.Extensions.Logging;

namespace CTA.WebForms2Blazor
{
    public class Program
    {
        //For demo purposes, allows us to perform migration without running CTA directly first
        public static async Task Main(string[] args)
        {
            string inputProjectDir = args[0];
            string outputProjectDir = args[1];
            string csProjFilePath = args[2];
            string slnPath = args[3];
            
            var ctaArgs = new[]
            {
                "-p", csProjFilePath, // can hardcode for local use
                "-v", "net5.0",                // set the Target Framework version
                "-d", "true",                         // use the default rules files (these will get downloaded from S3 and will tell CTA which packages to add to the new .csproj file)
                "-m", "false",                        // this is the "mock run" flag. Setting it to false means rules will be applied if we do a full port.
            };

            // Handle argument assignment
            PortCoreRulesCli cli = new PortCoreRulesCli();
            cli.HandleCommand(ctaArgs);
            
            var packageReferences = new Dictionary<string, Tuple<string, string>>
            {
                { "Autofac", new Tuple<string, string>("4.9.1.0", "4.9.3")},
                { "EntityFramework", new Tuple<string, string>("6.0.0.0", "6.4.4")},
                { "log4net", new Tuple<string, string>("2.0.8.0", "2.0.12")},
                { "Microsoft.Extensions.Logging.Log4Net.AspNetCore", new Tuple<string, string>("1.0.0", "2.2.12")}
            };
                
            // Create a configuration object using the CLI and other arbitrary values
            PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
            {
                ProjectPath = cli.FilePath,
                RulesDir = cli.RulesDir,
                IsMockRun = cli.IsMockRun,
                UseDefaultRules = cli.DefaultRules,
                PackageReferences = packageReferences,
                PortCode = false,
                PortProject = true,
                TargetVersions = new List<string> { cli.Version }
            };   
            
            var codeAnalyzer = CreateDefaultCodeAnalyzer();
            var analyzerResult = codeAnalyzer.AnalyzeProject(csProjFilePath).Result;

            var solutionPort = new SolutionPort(slnPath, new List<AnalyzerResult>() { analyzerResult }, new List<PortCoreConfiguration>() { projectConfiguration });

            MigrationManager migrationManager = new MigrationManager(inputProjectDir, outputProjectDir, analyzerResult, projectConfiguration);

            await migrationManager.PerformMigration();
        }
        
        
        private static CodeAnalyzer CreateDefaultCodeAnalyzer()
        {
            // Create a basic logger
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
            var logger = loggerFactory.CreateLogger("");

            // Set up analyzer config
            var configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings = {GenerateJsonOutput = false},
                MetaDataSettings =
                {
                    ReferenceData = true,
                    LoadBuildData = true,
                    GenerateBinFiles = true,
                    LocationData = false,
                    MethodInvocations = false,
                    LiteralExpressions = false,
                    LambdaMethods = false,
                    DeclarationNodes = false,
                    Annotations = false,
                    InterfaceDeclarations = false,
                    EnumDeclarations = false,
                    StructDeclarations = false,
                    ReturnStatements = false,
                    InvocationArguments = false,
                    ElementAccess = false,
                    MemberAccess = false
                }
            };

            return CodeAnalyzerFactory.GetAnalyzer(configuration, logger);
        }
        
    }
}
