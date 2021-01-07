using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codelyzer.Analysis;
using CTA.Rules.Config;

namespace CTA.Rules.Models
{
    public class PortSolutionResult : SolutionResult
    {
        public HashSet<string> References { get; set; }
        public HashSet<string> DownloadedFiles { get; set; }
        public string SolutionPath { get; set; }

        public PortSolutionResult(string solutionPath) : base()
        {
            SolutionPath = solutionPath;
            References = new HashSet<string>();
            DownloadedFiles = new HashSet<string>();
        }

        public PortSolutionResult(string solutionPath, SolutionResult solutionResult) : base()
        {
            SolutionPath = solutionPath;
            References = new HashSet<string>();
            DownloadedFiles = new HashSet<string>();
            this.ProjectResults = solutionResult.ProjectResults;
        }

        // Solution build errors grouped by project
        private Dictionary<string, Dictionary<string, int>> _buildErrors;
        public Dictionary<string, Dictionary<string, int>> BuildErrors
        {
            get
            {
                if (_buildErrors == null || !_buildErrors.Any())
                {
                    _buildErrors = GetSolutionBuildErrors();
                }

                return _buildErrors;
            }

            set
            {
                _buildErrors = value;
            }
        }

        public void AddSolutionResult(SolutionResult solutionResult)
        {
            this.ProjectResults = solutionResult.ProjectResults;
        }

        private Dictionary<string, Dictionary<string, int>> GetSolutionBuildErrors()
        {
            var buildErrorsByProject = new Dictionary<string, Dictionary<string, int>>();
            var codeAnalyzer = GetCodeAnalyzer();
            var analyzerResults = new List<AnalyzerResult>();
            try
            {
                analyzerResults = codeAnalyzer.AnalyzeSolution(SolutionPath).Result;
                
                // Process build errors by project
                foreach (var analyzerResult in analyzerResults)
                {
                    if (analyzerResult.ProjectResult != null)
                    {
                        // Count build errors by type
                        var buildErrorCounts = new Dictionary<string, int>();
                        var buildErrors = analyzerResult.ProjectResult.BuildErrors;
                        buildErrors.ForEach(e =>
                        {
                            string errorSeparator = ": error ";
                            var index = e.IndexOf(errorSeparator, StringComparison.InvariantCulture);
                            var s = e.Substring(index + errorSeparator.Length);

                            if (buildErrorCounts.ContainsKey(s))
                            {
                                buildErrorCounts[s]++;
                            }
                            else
                            {
                                buildErrorCounts.Add(s, 1);
                            }
                        });

                        var projectPath = analyzerResult.ProjectResult.ProjectFilePath;
                        buildErrorsByProject[projectPath] = buildErrorCounts;
                    }
                    analyzerResult.Dispose();
                }
            }
            catch (Exception)
            {
                foreach (var analyzerResult in analyzerResults)
                {
                    analyzerResult.Dispose();
                }
            }

            return buildErrorsByProject;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            
            // Print References
            str.AppendLine("==========");
            str.AppendLine(nameof(References)); 
            str.AppendLine("==========");
            str.AppendLine(string.Join(Environment.NewLine, References));
            str.AppendLine();

            // Print Downloaded Files
            str.AppendLine("===============");
            str.AppendLine(nameof(DownloadedFiles));
            str.AppendLine("===============");
            str.AppendLine(string.Join(Environment.NewLine, DownloadedFiles));
            str.AppendLine();

            // Print Project Results
            str.AppendLine("==============");
            str.AppendLine(nameof(ProjectResults));
            str.AppendLine("==============");

            foreach (var projectResult in ProjectResults)
            {
                str.AppendLine("----------------------");
                str.AppendLine($"Showing results for: {projectResult.ProjectFile}");
                str.AppendLine("----------------------");
                str.AppendLine(projectResult.ToString());
            }
            str.AppendLine();

            // Print Build Errors
            str.AppendLine("===========");
            str.AppendLine(nameof(BuildErrors));
            str.AppendLine("===========");
            str.AppendLine(BuildErrorsToString());
            return str.ToString();
        }

        private string BuildErrorsToString()
        {
            StringBuilder errorReport = new StringBuilder();
            foreach (var projectFile in BuildErrors.Keys)
            {
                errorReport.AppendLine("------------------");
                errorReport.AppendLine($"BUILD ERRORS FOR: {projectFile}");
                errorReport.AppendLine("------------------");
                errorReport.AppendLine();

                var buildErrorCounts = BuildErrors[projectFile];
                foreach (var buildError in buildErrorCounts.Keys)
                {
                    errorReport.AppendLine($"BuildError: {buildError}");
                    errorReport.AppendLine($"Count: {buildErrorCounts[buildError]}");
                    errorReport.AppendLine();
                }
                errorReport.AppendLine();
            }
            errorReport.AppendLine();
            return errorReport.ToString();
        }

        private CodeAnalyzer GetCodeAnalyzer()
        {
            AnalyzerConfiguration analyzerConfiguration = new AnalyzerConfiguration(LanguageOptions.CSharp);
            ExportSettings exportSettings = new ExportSettings() { GenerateGremlinOutput = false, GenerateJsonOutput = false, GenerateRDFOutput = false };
            MetaDataSettings metaDataSettings = new MetaDataSettings()
            {
                Annotations = false,
                DeclarationNodes = false,
                LambdaMethods = false,
                LiteralExpressions = false,
                LocationData = false,
                MethodInvocations = false,
                ReferenceData = false
            };
            analyzerConfiguration.ExportSettings = exportSettings;
            analyzerConfiguration.MetaDataSettings = metaDataSettings;

            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(analyzerConfiguration, LogHelper.Logger);
            return analyzer;
        }
    }
}
