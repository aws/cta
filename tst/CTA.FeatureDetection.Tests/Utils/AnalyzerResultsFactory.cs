using Codelyzer.Analysis;
using CTA.FeatureDetection.Common;
using System.Collections.Generic;
using System.IO;

namespace CTA.FeatureDetection.Tests.Utils
{
    public class AnalyzerResultsFactory
    {
        public static AnalyzerResult GetAnalyzerResult(string projectPath)
        {
            var codeAnalyzer = GetDefaultCodeAnalyzer(projectPath);
            return codeAnalyzer.AnalyzeProject(projectPath).Result;
        }

        public static IEnumerable<AnalyzerResult> GetAnalyzerResults(string solutionPath, int maxAttempts = 2)
        {
            var codeAnalyzer = GetDefaultCodeAnalyzer(solutionPath);
            return codeAnalyzer.AnalyzeSolution(solutionPath).Result;
        }

        private static CodeAnalyzer GetDefaultCodeAnalyzer(string solutionOrProjectPath)
        {
            // Codelyzer input
            var analyzerOutputDir = Path.Combine("..", "..");

            /* 1. Logger object */
            var logger = Log.Logger;

            /* 2. Get Analyzer instance based on language */
            var args = new[]
            {
                "-p", solutionOrProjectPath
            };
            AnalyzerCLI cli = new AnalyzerCLI();
            cli.HandleCommand(args);
            cli.Configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings =
                {
                    GenerateJsonOutput = false,
                    OutputPath = analyzerOutputDir
                },

                MetaDataSettings =
                {
                    LiteralExpressions = true,
                    MethodInvocations = true,
                    Annotations = true,
                    LambdaMethods = true,
                    DeclarationNodes = true,
                    LocationData = true,
                    ReferenceData = true,
                    LoadBuildData = true,
                    ReturnStatements = true
                }
            };

            return CodeAnalyzerFactory.GetAnalyzer(cli.Configuration, logger);
        }
    }
}
