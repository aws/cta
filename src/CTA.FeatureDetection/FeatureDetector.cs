using System;
using System.Collections.Generic;
using System.IO;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Common;
using CTA.FeatureDetection.Common;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Features;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using CTA.FeatureDetection.Load.Loaders;

namespace CTA.FeatureDetection
{
    public class FeatureDetector
    {
        public FeatureSet LoadedFeatureSet { get; private set; } = new FeatureSet();

        private static ILogger Logger => Log.Logger;

        public FeatureDetector() : this(FeatureSetLoader.LoadDefaultFeatureSet())
        {
        }

        public FeatureDetector(FeatureSet featureSet)
        {
            LoadedFeatureSet = featureSet;
        }

        public FeatureDetector(string featureConfigPath)
        {
            try
            {
                LoadedFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfigFile(featureConfigPath);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
            }
        }

        public FeatureDetector(IEnumerable<string> featureConfigPaths)
        {
            try
            {
                LoadedFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfigFiles(featureConfigPaths);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
            }
        }

        public FeatureDetector(FeatureConfig featureConfig) 
            : this(new [] { featureConfig })
        {
            try
            {
                LoadedFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfig(featureConfig);
            }
            catch (Exception e)
            {
                var serializedConfig = JsonConvert.SerializeObject(featureConfig);
                Logger.LogError(e, $"Failed to load features from feature config: {Environment.NewLine}{serializedConfig}");
            }
        }

        public FeatureDetector(IEnumerable<FeatureConfig> featureConfigs)
        {
            try
            {
                LoadedFeatureSet = FeatureSetLoader.LoadFeatureSetFromFeatureConfigs(featureConfigs);
            }
            catch (Exception e)
            {
                var serializedConfig = JsonConvert.SerializeObject(featureConfigs);
                Logger.LogError(e, $"Failed to load features from feature config: {Environment.NewLine}{serializedConfig}");
            }
        }

        /// <summary>
        /// Searches AnalyzerResult objects for specified features
        /// </summary>
        /// <param name="analyzerResults">Collection of analyzerResults to be searched for features</param>
        /// <returns>Dictionary of FeatureDetectionResults</returns>
        public Dictionary<string, FeatureDetectionResult> DetectFeaturesInProjects(IEnumerable<AnalyzerResult> analyzerResults)
        {
            var results = new Dictionary<string, FeatureDetectionResult>();

            foreach (var analyzerResult in analyzerResults)
            {
                var projectFilePath = analyzerResult.ProjectResult.ProjectFilePath;
                results[projectFilePath] = DetectFeaturesInProject(analyzerResult);
            }

            return results;
        }

        /// <summary>
        /// Searches AnalyzerResult object for specified features
        /// </summary>
        /// <param name="analyzerResult">AnalyzerResult object to be searched for features</param>
        /// <returns>FeatureDetectionResults containing information about the feature search</returns>
        public FeatureDetectionResult DetectFeaturesInProject(AnalyzerResult analyzerResult)
        {
            var result = new FeatureDetectionResult
            {
                ProjectPath = analyzerResult.ProjectResult.ProjectFilePath
            };

            foreach (var loadedFeature in LoadedFeatureSet.AllFeatures)
            {
                try
                {
                    result.FeatureStatus[loadedFeature.Name] = loadedFeature.IsPresent(analyzerResult);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Feature detection failed for {loadedFeature.GetType()}");
                }
            }

            return result;
        }

        /// <summary>
        /// Analyzes source code in project and searches for specified features
        /// </summary>
        /// <param name="projectPath">Project to search for features</param>
        /// <returns>FeatureDetectionResults containing information about the feature search</returns>
        public FeatureDetectionResult DetectFeaturesInProject(string projectPath)
        {
            var codeAnalyzer = GetDefaultCodeAnalyzer();
            var analyzerResult = codeAnalyzer.AnalyzeProject(projectPath).Result;
            var result = DetectFeaturesInProject(analyzerResult);

            return result;
        }

        /// <summary>
        /// Analyzes source code in project and searches for specified features
        /// </summary>
        /// <param name="solutionPath">Solution to search for features</param>
        /// <returns>Dictionary of FeatureDetectionResults containing information about the feature search</returns>
        public Dictionary<string, FeatureDetectionResult> DetectFeaturesInSolution(string solutionPath)
        {
            var codeAnalyzer = GetDefaultCodeAnalyzer();
            var analyzerResults = codeAnalyzer.AnalyzeSolution(solutionPath).Result;
            var result = DetectFeaturesInProjects(analyzerResults);

            return result;
        }

        private CodeAnalyzer GetDefaultCodeAnalyzer()
        {
            var analyzerOutputDir = Path.Combine("..", "..");
            var cli = new AnalyzerCLI
            {
                Configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
                {
                    ExportSettings =
                    {
                        GenerateJsonOutput = true,
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
                        LoadBuildData = true
                    }
                }
            };

            Logger.LogInformation("Creating default CodeAnalyzer with the following parameters:");
            Logger.LogInformation(cli.Project + " -- " + cli.FilePath);
            Logger.LogInformation(SerializeUtils.ToJson(cli.Configuration));

            /* Get Analyzer instance based on language */
            var analyzer = CodeAnalyzerFactory.GetAnalyzer(cli.Configuration, Logger);

            return analyzer;
        }
    }
}