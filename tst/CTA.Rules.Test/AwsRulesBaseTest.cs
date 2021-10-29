using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CTA.Rules.Test
{
    public class AwsRulesBaseTest
    {
        private string tstPath;
        private string srcPath;

        public static string CopyFolder = nameof(CopyFolder);

        protected static class TargetFramework
        {
            public const string Dotnet5 = "net5.0";
            public const string DotnetCoreApp31 = "netcoreapp3.1";
        }

        protected void Setup(System.Type type)
        {
            this.tstPath = GetTstPath(type);
            this.srcPath = GetSrcPath(type);
        }

        private string GetTstPath(System.Type type)
        {
            // The path will get normalized inside the .GetProject() call below
            string projectPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(type.Assembly.Location),
                    Path.Combine(new string[] { "..", "..", "..", ".." })));
            return projectPath;
        }

        private string GetSrcPath(System.Type type)
        {
            // The path will get normalized inside the .GetProject() call below
            string projectPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(type.Assembly.Location),
                    Path.Combine(new string[] { "..", "..", "..", "..", "..", "src" })));
            return projectPath;
        }

        public string GetTstPath(string path)
        {
            return Path.Combine(tstPath, path);
        }
        public string GetSrcPath(string path)
        {
            return Path.Combine(srcPath, path);
        }

        public TestSolutionAnalysis AnalyzeSolution(
            string solutionName,
            string tempDir,
            string downloadLocation,
            string version,
            Dictionary<string, List<string>> metaReferences = null,
            bool skipCopy = false,
            bool portCode = true,
            bool portProject = true)
        {
            string solutionPath = Directory.EnumerateFiles(tempDir, solutionName, SearchOption.AllDirectories).FirstOrDefault();

            if (!skipCopy)
            {
                solutionPath = CopySolutionFolderToTemp(solutionName, downloadLocation);
            }

            return AnalyzeSolution(solutionPath, version, metaReferences, true, portCode, portProject);
        }

        public TestSolutionAnalysis AnalyzeSolution(
            string solutionPath,
            string version,
            Dictionary<string, List<string>> metaReferences = null,
            bool skipCopy = false,
            bool portCode = true,
            bool portProject = true)
        {
            TestSolutionAnalysis result = new TestSolutionAnalysis();
            var solutionDir = Directory.GetParent(solutionPath).FullName;

            if (solutionPath != null && solutionPath.Length > 0)
            {
                List<PortCoreConfiguration> solutionPortConfiguration = new List<PortCoreConfiguration>();
                IEnumerable<string> projectFiles = Directory.EnumerateFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);

                if (projectFiles != null && projectFiles.Count() > 0)
                {
                    foreach (string projectFile in projectFiles)
                    {
                        Dictionary<string, Tuple<string, string>> packages = new Dictionary<string, Tuple<string, string>>
                        {
                            { "Newtonsoft.Json", new Tuple<string, string>("9.0.0", "*") }
                        };
                        PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
                        {
                            ProjectPath = projectFile,
                            UseDefaultRules = true,
                            TargetVersions = new List<string> { version },
                            PackageReferences = packages,
                            PortCode = portCode,
                            PortProject = portProject
                        };

                        if (metaReferences != null)
                        {
                            projectConfiguration.MetaReferences = metaReferences.ContainsKey(projectFile) ? metaReferences[projectFile] : null;
                        }

                        solutionPortConfiguration.Add(projectConfiguration);
                    }
                    
                    // SolutionPort should remove this extra config because it does not have a matching analyzer result.
                    // Otherwise will hit KeyNotFoundException 
                    solutionPortConfiguration.Add(new PortCoreConfiguration
                    {
                        ProjectPath = "fakeproject.csproj",
                        UseDefaultRules = true,
                    });

                    SolutionPort solutionPort = new SolutionPort(solutionPath, solutionPortConfiguration);
                    CopyTestRules();
                    var analysisRunResult = solutionPort.AnalysisRun();

                    foreach (var projectResult in analysisRunResult.ProjectResults)
                    {
                        Assert.IsTrue(projectResult.ProjectActions.ToSummaryString()?.Length > 0);
                    }
                    var runResult = solutionPort.Run();
                    result = GenerateSolutionResult(solutionDir, analysisRunResult, runResult);
                }
            }
            return result;
        }

        internal TestSolutionAnalysis GenerateSolutionResult(string solutionDir, SolutionResult analysisRunResult, PortSolutionResult solutionRunResult)
        {
            var result = new TestSolutionAnalysis();

            var projectFiles = Directory.EnumerateFiles(solutionDir, "*.csproj", SearchOption.AllDirectories).ToList();
            projectFiles.ForEach(projectFile => {
                result.ProjectResults.Add(new ProjectResult()
                {
                    CsProjectPath = projectFile,
                    ProjectDirectory = Directory.GetParent(projectFile).FullName,
                    CsProjectContent = File.ReadAllText(projectFile)
                });
            });

            StringBuilder str = new StringBuilder();
            foreach (var projectResult in analysisRunResult.ProjectResults)
            {
                StringBuilder projectResults = new StringBuilder();
                projectResults.AppendLine(projectResult.ProjectFile);
                projectResults.AppendLine(projectResult.ProjectActions.ToString());
                result.ProjectResults.Where(p => p.CsProjectPath == projectResult.ProjectFile).FirstOrDefault().ProjectAnalysisResult = projectResults.ToString();

                str.Append(projectResults);
            }

            result.SolutionAnalysisResult = str.ToString();
            result.SolutionRunResult = solutionRunResult;

            return result;
        }

        private void CopyTestRules()
        {
            var tempRulesDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TempRules");
            if (Directory.Exists(tempRulesDir))
            {
                var files = Directory.EnumerateFiles(tempRulesDir, "*.json");

                foreach (var file in files)
                {
                    string targetFile = Path.Combine(Constants.RulesDefaultPath, Path.GetFileName(file));
                    File.Copy(file, targetFile, true);
                }
            }
        }

        public string DownloadTestProjects(string tempDir)
        {
            var tempDirectory = Directory.CreateDirectory(tempDir);
            string downloadLocation = Path.Combine(tempDirectory.FullName, "d");

            var fileName = Path.Combine(tempDirectory.Parent.FullName, @"TestProjects.zip");
            Utils.SaveFileFromGitHub(fileName, GithubInfo.TestGithubOwner, GithubInfo.TestGithubRepo, GithubInfo.TestGithubTag);
            ZipFile.ExtractToDirectory(fileName, downloadLocation, true);
            return downloadLocation;
        }

        protected void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            var files = source.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }

            var dirs = source.GetDirectories();
            foreach (var dir in dirs)
            {
                DirectoryInfo destinationSub = new DirectoryInfo(Path.Combine(target.FullName, dir.Name));
                CopyDirectory(dir, destinationSub);
            }
        }

        protected string CopySolutionFolderToTemp(string solutionName, string tempDir)
        {
            string solutionPath = Directory.EnumerateFiles(tempDir, solutionName, SearchOption.AllDirectories).FirstOrDefault(s => !s.Contains(string.Concat(Path.DirectorySeparatorChar, CopyFolder, Path.DirectorySeparatorChar)));
            string solutionDir = Directory.GetParent(solutionPath).FullName;
            var newTempDir = Path.Combine(GetTstPath(this.GetType()), CopyFolder, Guid.NewGuid().ToString());
            CopyDirectory(new DirectoryInfo(solutionDir), new DirectoryInfo(newTempDir));

            solutionPath = Directory.EnumerateFiles(newTempDir, solutionName, SearchOption.AllDirectories).FirstOrDefault();
            return solutionPath;
        }


        protected List<string> GetSolutionBuildErrors(string solutionPath)
        {
            var result = GetBuildResults(solutionPath);

            var allErrors = new List<string>();
            result.ForEach(r => allErrors.AddRange(r.ProjectBuildResult.BuildErrors));
            return allErrors;
        }

        protected List<AnalyzerResult> GetBuildResults(string solutionPath)
        {
            AnalyzerConfiguration configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings =
                {
                    GenerateJsonOutput = false,
                    OutputPath = @"/tmp/UnitTests"
                },

                MetaDataSettings =
                {
                    LiteralExpressions = true,
                    MethodInvocations = true,
                    Annotations = true,
                    DeclarationNodes = true,
                    LocationData = false,
                    ReferenceData = true,
                    LoadBuildData = true,
                    ElementAccess = true,
                    MemberAccess = true
                }
            };
            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(configuration, NullLogger.Instance);
            var result = analyzer.AnalyzeSolution(solutionPath).Result;
            return result;
        }

        protected List<AnalyzerResult> GenerateSolutionAnalysis(string solutionPath)
        {
            AnalyzerConfiguration configuration = new AnalyzerConfiguration(LanguageOptions.CSharp)
            {
                ExportSettings =
                {
                    GenerateJsonOutput = false,
                    OutputPath = @"/tmp/UnitTests"
                },

                MetaDataSettings =
                {
                    LiteralExpressions = true,
                    MethodInvocations = true,
                    Annotations = true,
                    DeclarationNodes = true,
                    LocationData = false,
                    ReferenceData = true,
                    LoadBuildData = true,
                    ElementAccess = true,
                    MemberAccess = true
                }
            };
            CodeAnalyzer analyzer = CodeAnalyzerFactory.GetAnalyzer(configuration, NullLogger.Instance);
            var result = analyzer.AnalyzeSolution(solutionPath).Result;
            return result;
        }
    }
}

