﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.ProjectType.Extensions;
using CTA.Rules.Config;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using CTA.Rules.Update;
using Microsoft.Extensions.Logging;

namespace CTA.Rules.PortCore
{
    /// <summary>
    /// Ports a solution
    /// </summary>
    public class ProjectPort
    {
        private ProjectRewriter _projectRewriter;
        internal FeatureDetectionResult ProjectTypeFeatureResults;
        private ProjectResult _projectResult;
        internal HashSet<string> ProjectReferences;
        private SolutionPort _solutionPort;

        public ProjectPort(AnalyzerResult analyzerResult, PortCoreConfiguration projectConfiguration, SolutionPort solutionPort, ILogger logger = null)
        {
            if (logger != null)
            {
                LogHelper.Logger = logger;
            }

            _solutionPort = solutionPort;
            ProjectReferences = new HashSet<string>() { Constants.ProjectRecommendationFile };
            InitProjectRewriter(analyzerResult, projectConfiguration);
        }
        private void InitProjectRewriter(AnalyzerResult analyzerResult, PortCoreConfiguration projectConfiguration)
        {
            InitRules(projectConfiguration, analyzerResult);
            _projectRewriter = new ProjectRewriter(analyzerResult, projectConfiguration);
        }

        private void InitRules(PortCoreConfiguration projectConfiguration, AnalyzerResult analyzerResult)
        {
            using var projectTypeFeatureDetector = new FeatureDetector();

            ProjectTypeFeatureResults = projectTypeFeatureDetector.DetectFeaturesInProject(analyzerResult);

            projectConfiguration.ProjectType = _solutionPort.GetProjectType(ProjectTypeFeatureResults);
            if (projectConfiguration.UseDefaultRules)
            {
                //If a rules dir was provided, copy files from that dir into the rules folder
                if (!string.IsNullOrEmpty(projectConfiguration.RulesDir))
                {
                    _solutionPort.CopyOverrideRules(projectConfiguration.RulesDir);
                }
                projectConfiguration.RulesDir = Constants.RulesDefaultPath;
                var projectResult = analyzerResult.ProjectResult;

                projectResult?.SourceFileResults?.SelectMany(s => s.References)?.Select(r => r.Namespace).Distinct().ToList().ForEach(currentReference =>
                {
                    if (currentReference != null && !ProjectReferences.Contains(currentReference))
                    {
                        ProjectReferences.Add(currentReference);
                    }
                });

                projectResult?.SourceFileResults?.SelectMany(s => s.Children.OfType<UsingDirective>())?.Select(u => u.Identifier).Distinct().ToList().ForEach(currentReference =>
                {
                    if (currentReference != null && !ProjectReferences.Contains(currentReference))
                    {
                        ProjectReferences.Add(currentReference);
                    }
                });
            }

            _solutionPort.DownloadRecommendationFiles(ProjectReferences);

        }
        /// <summary>
        /// Initializes the Solution Port
        /// </summary>
        public ProjectResult AnalysisRun()
        {
            // If the solution was already analyzed, don't duplicate the results
            if (_projectResult != null) 
            {
                return _projectResult;
            }

            _projectResult = _projectRewriter.Initialize();

            return _projectResult;
        }

        /// <summary>
        /// Runs the Solution Port after creating an analysis
        /// </summary>
        public ProjectResult Run()
        {
            // Find actions to execute for each project
            var projectAnalysisResult = AnalysisRun();
            var projectActions = projectAnalysisResult.ProjectActions;

            // Pass in the actions found to translate all files in each project
            var projectRewriterResult = _projectRewriter.Run(projectActions);
            return projectRewriterResult;
        }
    }
}
