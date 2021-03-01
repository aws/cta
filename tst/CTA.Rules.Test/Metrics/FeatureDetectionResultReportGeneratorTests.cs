using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Metrics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using CTA.FeatureDetection.Common.Models;

namespace CTA.Rules.Test.Metrics
{
    public class FeatureDetectionResultReportGeneratorTests
    {
        private const string _tempDir = "temp";
        public FeatureDetectionResultReportGenerator ReportGenerator;

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(_tempDir);
            var solutionPath = $"{_tempDir}/solution.sln";
            var projectPath1 = $"{_tempDir}/project1.csproj";
            var projectPath2 = $"{_tempDir}/project2.csproj";
            var projectResult1 = new ProjectWorkspace(projectPath1)
            {
                ProjectGuid = "1234-5678"
            };
            var analyzerResult1 = new AnalyzerResult
            {
                ProjectResult = projectResult1
            };
            var projectResult2 = new ProjectWorkspace(projectPath2)
            {
                ProjectGuid = "ABCD-EFGH"
            };
            var analyzerResult2 = new AnalyzerResult
            {
                ProjectResult = projectResult2
            };
            var analyzerResults = new List<AnalyzerResult>
            {
                analyzerResult1,
                analyzerResult2
            };
            var context = new MetricsContext(solutionPath, analyzerResults);

            var featureDetectionResults = new Dictionary<string, FeatureDetectionResult>
            {
                { projectPath1, new FeatureDetectionResult
                    {
                        FeatureStatus =
                        {
                            { "Feature 1", true },
                            { "Feature 1a", true },
                            { "Feature 1b", false },
                        },
                        ProjectPath = projectPath1
                    }
                },
                { projectPath2, new FeatureDetectionResult
                    {
                        FeatureStatus =
                        {
                            { "Feature 2", false },
                            { "Feature 2a", false },
                            { "Feature 2b", false },
                        },
                        ProjectPath = projectPath2
                    }
                },
            };
            
            ReportGenerator = new FeatureDetectionResultReportGenerator(context, featureDetectionResults);
            ReportGenerator.GenerateFeatureDetectionReport();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_tempDir, true);
        }

        [Test]
        public void FeatureDetectionReport_Creates_Expected_Json_Report()
        {
            var expectedFeatureDetectionReport = @"[
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""DetectedFeature"",
    ""featureName"": ""Feature 1"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""DetectedFeature"",
    ""featureName"": ""Feature 1a"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  }
]";
            var formattedReport = JToken.Parse(ReportGenerator.FeatureDetectionResultJsonReport.Trim()).ToString(Formatting.Indented);
            Assert.AreEqual(expectedFeatureDetectionReport, formattedReport);
        }
    }
}