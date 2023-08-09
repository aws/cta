using Codelyzer.Analysis;
using Codelyzer.Analysis.Analyzer;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common;
using System.Collections.Generic;
using System.IO;

namespace CTA.FeatureDetection.Tests.Utils
{
    public class AnalyzerResultsFactory
    {
        public static AnalyzerResult GetAnalyzerResult(string projectPath, string language = LanguageOptions.CSharp)
        {
            var codeAnalyzer = GetDefaultCodeAnalyzer(projectPath, language);
            return codeAnalyzer.AnalyzeProject(projectPath).Result;
        }

        public static IEnumerable<AnalyzerResult> GetAnalyzerResults(string solutionPath, string language = LanguageOptions.CSharp)
        {
            var codeAnalyzer = GetDefaultCodeAnalyzer(solutionPath, language);
            return codeAnalyzer.AnalyzeSolution(solutionPath).Result;
        }

        private static CodeAnalyzer GetDefaultCodeAnalyzer(string solutionOrProjectPath, string language = LanguageOptions.CSharp)
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
            cli.Configuration = new AnalyzerConfiguration(language, null)
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
                    ReturnStatements = true,
                    InterfaceDeclarations = true
                }
            };

            return CodeAnalyzerFactory.GetAnalyzer(cli.Configuration, logger);
            //return new CodeAnalyzerByLanguage(cli.Configuration, logger);

        }
    }
}
