using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Config;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using CTA.FeatureDetection.Common.Models;

namespace CTA.Rules.Test.Metrics
{
    public class PortSolutionResultReportGeneratorTests
    {
        private const string _tempDir = "temp";
        public PortSolutionResultReportGenerator ReportGenerator;
        public PortSolutionResultReportGenerator ReportGeneratorWithFeatureDetection;

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
            var projectResult2 = new ProjectWorkspace(projectPath2)
            {
                ProjectGuid = "ABCD-EFGH"
            };
            var analyzerResult1 = new AnalyzerResult
            {
                ProjectResult = projectResult1
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

            var buildErrors = new Dictionary<string, Dictionary<string, int>>
            {
                { projectPath1, new Dictionary<string, int>
                    {
                        { "CS0000: BuildError1", 100 },
                        { "BuildError2", 200 }
                    }
                },
                { "ProjectFilePath2", new Dictionary<string, int>
                    {
                        { "BuildError3", 300 },
                        { "BuildError4", 400 }
                    }
                }
            };
            var references = new HashSet<string>
            {
                "System.Web",
                "System.Web.Mvc"
            };
            var downloadedFiles = new HashSet<string>
            {
                "project.all.json"
            };
            var projectResults = new BlockingCollection<CTA.Rules.Models.ProjectResult>
            {
                new CTA.Rules.Models.ProjectResult
                {
                    ProjectFile = projectPath1,
                    TargetVersions = new List<string>() {Constants.DefaultCoreVersion},
                    UpgradePackages = new List<PackageAction>() {new PackageAction() { Name = "Newtonsoft.Json", OriginalVersion="9.0.0", Version="12.0.0" } },
                    ProjectActions = new ProjectActions() {
                        FileActions = new BlockingCollection<FileActions>()
                        {
                            new FileActions()
                            {
                                FilePath="FilePath1",
                                AttributeActions = new HashSet<AttributeAction>()
                                {
                                    new AttributeAction()
                                    {
                                        Key = "SampleKey1",
                                        Type = "GA1 Type",
                                        Name = "GA1 Name",
                                        Value = "GA1 Value",
                                    },
                                    new AttributeAction()
                                    {
                                        Key = "SampleKey2",
                                        Type = "GA2 Type",
                                        Name = "GA2 Name",
                                        Value = "GA2 Value",
                                    }
                                }
                            }
                        }
                    },
                    ExecutedActions = new Dictionary<string, List<GenericActionExecution>>
                    {
                        { "FilePath1", new List<GenericActionExecution>
                            {
                                new GenericActionExecution
                                {
                                    Type = "GA1 Type",
                                    Name = "GA1 Name",
                                    Value = "GA1 Value",
                                    TimesRun = 2,
                                    InvalidExecutions = 1
                                },
                                new GenericActionExecution
                                {
                                    Type = "GA2 Type",
                                    Name = "GA2 Name",
                                    Value = "GA2 Value",
                                    TimesRun = 3,
                                    InvalidExecutions = 0
                                }
                            }
                        }
                    }
                }
            };
            var portSolutionResult = new PortSolutionResult(solutionPath)
            {
                BuildErrors = buildErrors,
                References = references,
                DownloadedFiles = downloadedFiles,
                ProjectResults = projectResults
            };

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

            ReportGenerator = new PortSolutionResultReportGenerator(context, portSolutionResult);
            ReportGenerator.GenerateAnalysisReport();
            ReportGenerator.GenerateAndExportReports();

            ReportGeneratorWithFeatureDetection = new PortSolutionResultReportGenerator(context, portSolutionResult, featureDetectionResults);
            ReportGeneratorWithFeatureDetection.GenerateAnalysisReport();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_tempDir, true);
        }

        [Test]
        public void GenerateAnalysisReport()
        {
            var expectedAnalysisReport = @"[
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""UpgradePackage"",
    ""packageName"": ""Newtonsoft.Json"",
    ""packageVersion"": ""12.0.0"",
    ""packageOriginalVersion"": ""9.0.0"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""GenericAction"",
    ""actionName"": ""GA1 Name"",
    ""actionType"": ""GA1 Type"",
    ""actionValue"": ""GA1 Value"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678"",
    ""filePath"": ""eb98c1d648bc61064bdeaca9523a49e51bb3312f28f59376fb385e1569c77822""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""GenericAction"",
    ""actionName"": ""GA2 Name"",
    ""actionType"": ""GA2 Type"",
    ""actionValue"": ""GA2 Value"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678"",
    ""filePath"": ""eb98c1d648bc61064bdeaca9523a49e51bb3312f28f59376fb385e1569c77822""
  }
]";
            var formattedReport = JValue.Parse(ReportGenerator.AnalyzeSolutionResultJsonReport.Trim()).ToString(Formatting.Indented);
            Assert.AreEqual(expectedAnalysisReport, formattedReport);
        }

        [Test]
        public void GenerateAnalysisReportWithFeatureDetection()
        {
            var expectedAnalysisReport = @"[
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""UpgradePackage"",
    ""packageName"": ""Newtonsoft.Json"",
    ""packageVersion"": ""12.0.0"",
    ""packageOriginalVersion"": ""9.0.0"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""GenericAction"",
    ""actionName"": ""GA1 Name"",
    ""actionType"": ""GA1 Type"",
    ""actionValue"": ""GA1 Value"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678"",
    ""filePath"": ""eb98c1d648bc61064bdeaca9523a49e51bb3312f28f59376fb385e1569c77822""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""GenericAction"",
    ""actionName"": ""GA2 Name"",
    ""actionType"": ""GA2 Type"",
    ""actionValue"": ""GA2 Value"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678"",
    ""filePath"": ""eb98c1d648bc61064bdeaca9523a49e51bb3312f28f59376fb385e1569c77822""
  },
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
            var formattedReport = JValue.Parse(ReportGeneratorWithFeatureDetection.AnalyzeSolutionResultJsonReport.Trim()).ToString(Formatting.Indented);
            Assert.AreEqual(expectedAnalysisReport, formattedReport);
        }

        [Test]
        public void GenerateAndExportReports_Creates_Expected_Text_Report()
        {
            var expectedTextReport = @"==========
References
==========
System.Web
System.Web.Mvc

===============
DownloadedFiles
===============
project.all.json

==============
ProjectResults
==============
----------------------
Showing results for: temp/project1.csproj
----------------------
---------------------------
Target Versions:
---------------------------
netcoreapp3.1
---------------------------
Upgrade packages:
---------------------------
Newtonsoft.Json,9.0.0->12.0.0
---------------------------
Action packages:
---------------------------
---------------------------
Executed Actions for file: FilePath1
---------------------------

Action Type: GA1 Type
Action Name: GA1 Name
Action Value: GA1Value
Times Run: 2
Invalid Executions: 1

Action Type: GA2 Type
Action Name: GA2 Name
Action Value: GA2Value
Times Run: 3
Invalid Executions: 0



===========
BuildErrors
===========
------------------
BUILD ERRORS FOR: temp/project1.csproj
------------------

BuildError: CS0000: BuildError1
Count: 100

BuildError: BuildError2
Count: 200


------------------
BUILD ERRORS FOR: ProjectFilePath2
------------------

BuildError: BuildError3
Count: 300

BuildError: BuildError4
Count: 400";

            Assert.AreEqual(expectedTextReport, ReportGenerator.PortSolutionResultTextReport.Trim());
        }

        [Test]
        public void GenerateAndExportReports_Creates_Expected_Json_Report()
        {
            var expectedJsonReport = @"[
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""Namespace"",
    ""reference"": ""System.Web"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""Namespace"",
    ""reference"": ""System.Web.Mvc"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""RulesFile"",
    ""downloadedFile"": ""project.all.json"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""TargetVersion"",
    ""targetVersion"": ""netcoreapp3.1"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""UpgradePackage"",
    ""packageName"": ""Newtonsoft.Json"",
    ""packageVersion"": ""12.0.0"",
    ""packageOriginalVersion"": ""9.0.0"",
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""ActionExecution"",
    ""actionName"": ""GA1 Name"",
    ""actionType"": ""GA1 Type"",
    ""actionValue"": ""GA1 Value"",
    ""timesRun"": 2,
    ""invalidExecutions"": 1,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678"",
    ""filePath"": ""N/A""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""ActionExecution"",
    ""actionName"": ""GA2 Name"",
    ""actionType"": ""GA2 Type"",
    ""actionValue"": ""GA2 Value"",
    ""timesRun"": 3,
    ""invalidExecutions"": 0,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678"",
    ""filePath"": ""N/A""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""BuildError"",
    ""buildErrorCode"": ""CS0000"",
    ""buildError"": ""CS0000: BuildError1"",
    ""count"": 100,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""BuildError"",
    ""buildErrorCode"": ""OTHER"",
    ""buildError"": ""BuildError2"",
    ""count"": 200,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""BuildError"",
    ""buildErrorCode"": ""OTHER"",
    ""buildError"": ""BuildError3"",
    ""count"": 300,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""N/A""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""BuildError"",
    ""buildErrorCode"": ""OTHER"",
    ""buildError"": ""BuildError4"",
    ""count"": 400,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""N/A""
  }
]";
            var unformattedReport = ReportGenerator.PortSolutionResultJsonReport.Trim();
            var formattedReport = JValue.Parse(unformattedReport).ToString(Formatting.Indented);
            Assert.AreEqual(expectedJsonReport, formattedReport);
        }
    }
}