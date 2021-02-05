using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Config;
using CTA.Rules.Metrics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CTA.Rules.Test.Metrics
{
    public class MetricsModelTests
    {
        public string SolutionPath { get; set; }
        public string ProjectPath { get; set; }
        public MetricsContext Context { get; set; }

        [SetUp]
        public void Setup()
        {
            SolutionPath = "temp/solutionPath";
            ProjectPath = "temp/solutionPath";

            var projectResult = new ProjectWorkspace(ProjectPath)
            {
                ProjectGuid = "1234-5678"
            };
            var analyzerResult = new AnalyzerResult
            {
                ProjectResult = projectResult
            };
            var analyzerResults = new List<AnalyzerResult>
            {
                analyzerResult
            };

            Context = new MetricsContext(SolutionPath, analyzerResults);


        }

        [Test]
        public void MetricsContext_Populates_Fields_Correctly()
        {
            Assert.True(Context.SolutionPath == SolutionPath);
            Assert.True(Context.SolutionPathHash == EncryptionHelper.ConvertToSHA256Hex(SolutionPath));
            Assert.True(Context.ProjectGuidMap.Count == 1);
            Assert.True(Context.ProjectGuidMap.First().Key == ProjectPath);
        }
    }
}