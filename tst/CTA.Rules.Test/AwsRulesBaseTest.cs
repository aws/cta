using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CTA.Rules.Test
{
    public class AwsRulesBaseTest
    {
        private string tstPath;
        private string srcPath;

        public static string CopyFolder = nameof(CopyFolder);

        protected static class TargetFramework
        {
            public const string DotnetCoreApp31 = "netcoreapp3.1";
            public const string Dotnet5 = "net5.0";
            public const string Dotnet6 = "net6.0";
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

            if (solutionPath != null && solutionPath.Length > 0)
            {
                List<PortCoreConfiguration> solutionPortConfiguration = new List<PortCoreConfiguration>();
                IEnumerable<string> projectFiles = Utils.GetProjectPaths(solutionPath);

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
                            SolutionPath = solutionPath,
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
                        result.ProjectResults.Add(new ProjectResult()
                        {
                            CsProjectPath = projectFile,
                            ProjectDirectory = Directory.GetParent(projectFile).FullName
                        });
                    }

                    // SolutionPort should remove this extra config because it does not have a matching analyzer result.
                    // Otherwise will hit KeyNotFoundException 
                    solutionPortConfiguration.Add(new PortCoreConfiguration
                    {
                        SolutionPath = solutionPath,
                        ProjectPath = "fakeproject.csproj",
                        UseDefaultRules = true,
                    });

                    SolutionPort solutionPort = new SolutionPort(solutionPath, solutionPortConfiguration);
                    CopyTestRules();
                    CopyTestTemplates();
                    CopyTestTagConfigs();
                    var analysisRunResult = solutionPort.AnalysisRun();

                    StringBuilder str = new StringBuilder();
                    foreach (var projectResult in analysisRunResult.ProjectResults)
                    {
                        Assert.IsTrue(projectResult.ProjectActions.ToSummaryString()?.Length > 0);
                        StringBuilder projectResults = new StringBuilder();
                        projectResults.AppendLine(projectResult.ProjectFile);
                        projectResults.AppendLine(projectResult.ProjectActions.ToString());
                        result.ProjectResults.Where(p => p.CsProjectPath == projectResult.ProjectFile).FirstOrDefault().ProjectAnalysisResult = projectResults.ToString();

                        str.Append(projectResults);
                    }
                    result.SolutionAnalysisResult = str.ToString();

                    var runResult = solutionPort.Run();

                    foreach (var projectFile in result.ProjectResults)
                    {
                        projectFile.CsProjectContent = File.ReadAllText(projectFile.CsProjectPath);
                    }

                    result.SolutionRunResult = runResult;
                }
            }
            return result;
        }

        internal TestSolutionAnalysis GenerateSolutionResult(string solutionPath, SolutionResult analysisRunResult, PortSolutionResult solutionRunResult)
        {
            var result = new TestSolutionAnalysis();

            var projectFiles = Utils.GetProjectPaths(solutionPath).ToList();
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

        protected void CopyTestRules()
        {
            // Project configured to copy TempRules folder to output directory
            // so no extra action necessary here
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var tempRulesDir = Path.Combine(assemblyDir, "TempRules");
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

        private void CopyTestTemplates()
        {
            // Project configured to copy TempTemplates folder to output directory
            // so no extra action necessary here
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var tempTemplatesDir = Path.Combine(assemblyDir, "TempTemplates");
            if (Directory.Exists(tempTemplatesDir))
            {
                var files = Directory.EnumerateFiles(tempTemplatesDir, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(tempTemplatesDir, file);
                    var targetFile = Path.Combine(Constants.ResourcesExtractedPath, relativePath);
                    var targetFileDir = Path.GetDirectoryName(targetFile);

                    Directory.CreateDirectory(targetFileDir);
                    File.Copy(file, targetFile, true);
                }
            }
        }

        private void CopyTestTagConfigs()
        {
            // Project configured to copy TempTagConfigs folder to output directory
            // so no extra action necessary here
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var tempTemplatesDir = Path.Combine(assemblyDir, "TempTagConfigs");
            if (Directory.Exists(tempTemplatesDir))
            {
                var files = Directory.EnumerateFiles(tempTemplatesDir, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(tempTemplatesDir, file);
                    var targetFile = Path.Combine(Rules.Config.Constants.TagConfigsExtractedPath, relativePath);
                    var targetFileDir = Path.GetDirectoryName(targetFile);

                    Directory.CreateDirectory(targetFileDir);
                    File.Copy(file, targetFile, true);
                }
            }
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
                file.CopyTo(Path.Combine(target.FullName, file.Name),true);
            }

            var dirs = source.GetDirectories();
            foreach (var dir in dirs)
            {
                DirectoryInfo destinationSub = new DirectoryInfo(Path.Combine(target.FullName, dir.Name));
                CopyDirectory(dir, destinationSub);
            }
        }

        protected string CopySolutionFolderToTemp(string solutionName, string searchDir)
        {
            var solutionPath = Directory.EnumerateFiles(searchDir, solutionName, SearchOption.AllDirectories).FirstOrDefault(s => !s.Contains(string.Concat(Path.DirectorySeparatorChar, CopyFolder, Path.DirectorySeparatorChar)));
            var solutionDir = Directory.GetParent(solutionPath).FullName;
            var newTempDir = Path.Combine(GetTstPath(this.GetType()), CopyFolder, Guid.NewGuid().ToString());

            if (solutionPath.Contains(".sln") && File.Exists(solutionPath))
            {
                IEnumerable<string> projects = Utils.GetProjectPaths(solutionPath);

                newTempDir = BuildRelativeFolderStructureToIncludeAllExternalProjects(projects, newTempDir, solutionDir);

                CopyDirectory(new DirectoryInfo(solutionDir), new DirectoryInfo(newTempDir));

                FindAndCopyProjectsOutsideSolutionPath(projects, solutionDir, newTempDir);
            }
            else
            {
                CopyDirectory(new DirectoryInfo(solutionDir), new DirectoryInfo(newTempDir));
            }

            var newSolutionPath = Directory.EnumerateFiles(newTempDir, solutionName, SearchOption.AllDirectories).FirstOrDefault();
            return newSolutionPath;
        }

        private string BuildRelativeFolderStructureToIncludeAllExternalProjects(IEnumerable<string> projects, string newTempDir, string solutionDir)
        {
            int folderCount = 1;
            int depths = projects.ToList().Max(p => Regex.Matches(Path.GetRelativePath(solutionDir, p), Regex.Escape("..")).Count);
            for (int i = 0; i < depths; i++)
            {
                newTempDir += "\\Folder" + folderCount++;
            }
            return newTempDir;
        }

        private void FindAndCopyProjectsOutsideSolutionPath(IEnumerable<string> projects, string solutionDir, string newTempDir)
        {
            foreach (string project in projects)
            {
                string projPath = Directory.GetParent(project).FullName;

                if (!Utils.IsSubPathOf(solutionDir, projPath))
                {
                    string relativeSrc = Path.GetRelativePath(solutionDir, projPath);
                    string projName = Path.GetFileName(project);
                    string newRelDir = Path.Combine(newTempDir, relativeSrc);

                    Utils.CopyFolderToTemp(projName, projPath, newRelDir);
                }
            }
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

