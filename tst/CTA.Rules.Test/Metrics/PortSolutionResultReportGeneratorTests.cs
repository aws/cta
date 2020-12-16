using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CTA.Rules.Test.Metrics
{
    public class PortSolutionResultReportGeneratorTests
    {
        private const string _tempDir = "temp";
        public PortSolutionResultReportGenerator ReportGenerator;

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(_tempDir);
            var solutionPath = $"{_tempDir}/solution.sln";
            var projectPath = $"{_tempDir}/project.csproj";
            var projectResult = new ProjectWorkspace(projectPath)
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
            var context = new MetricsContext(solutionPath, analyzerResults);

            var buildErrors = new Dictionary<string, Dictionary<string, int>>
            {
                { projectPath, new Dictionary<string, int>
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
            var projectResults = new BlockingCollection<Models.ProjectResult>
            {
                new Models.ProjectResult
                {
                    ProjectFile = projectPath,
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

            ReportGenerator = new PortSolutionResultReportGenerator(context, portSolutionResult);
            ReportGenerator.GenerateAndExportReports();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_tempDir, true);
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
Showing results for: temp/project.csproj
----------------------
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
BUILD ERRORS FOR: temp/project.csproj
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
    ""buildErrorCode"": """",
    ""buildError"": ""BuildError2"",
    ""count"": 200,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""1234-5678""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""BuildError"",
    ""buildErrorCode"": """",
    ""buildError"": ""BuildError3"",
    ""count"": 300,
    ""solutionPath"": ""5fa9de0cb5af2d468dfb1702b1e342f47de2df9a195dabb3be2d04f9c2767482"",
    ""projectGuid"": ""N/A""
  },
  {
    ""metricsType"": ""CTA"",
    ""metricName"": ""BuildError"",
    ""buildErrorCode"": """",
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