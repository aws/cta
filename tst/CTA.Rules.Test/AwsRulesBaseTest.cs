using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using CTA.Rules.Update;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CTA.Rules.Test
{
    public class AwsRulesBaseTest
    {
        private System.Type systemType;
        private string tstPath;
        private string srcPath;

        protected void Setup(System.Type type)
        {
            this.systemType = type;
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

        public TestSolutionAnalysis AnalyzeSolution(string solutionName, string tempDir, string downloadLocation, string version, bool skipCopy = false)
        {
            TestSolutionAnalysis result = new TestSolutionAnalysis();

            var sourceDir = Directory.GetParent(Directory.EnumerateFiles(downloadLocation, solutionName, SearchOption.AllDirectories).FirstOrDefault());
            var solutionDir = Path.Combine(tempDir, version);

            if (!skipCopy)
            {
                CopyDirectory(sourceDir, new DirectoryInfo(solutionDir));
            }
            else
            {
                solutionDir = tempDir;
            }
            string solutionPath = Directory.EnumerateFiles(solutionDir, solutionName, SearchOption.AllDirectories).FirstOrDefault();

            if (solutionPath != null && solutionPath.Length > 0)
            {
                List<PortCoreConfiguration> solutionPortConfiguration = new List<PortCoreConfiguration>();
                IEnumerable<string> projectFiles = Directory.EnumerateFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);

                if (projectFiles != null && projectFiles.Count() > 0)
                {
                    foreach (string projectFile in projectFiles)
                    {
                        ProjectResult project = new ProjectResult();

                        Dictionary<string, Tuple<string, string>> packages = new Dictionary<string, Tuple<string, string>>();
                        packages.Add("Newtonsoft.Json", new Tuple<string, string>("9.0.0", "*"));
                        PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
                        {
                            ProjectPath = projectFile,
                            UseDefaultRules = true,
                            TargetVersions = new List<string> { version },
                            PackageReferences = packages
                        };

                        //project.CsProjectContent = File.ReadAllText(projectFile);
                        project.ProjectDirectory = Directory.GetParent(projectFile).FullName;
                        project.CsProjectPath = projectFile;

                        result.ProjectResults.Add(project);

                        solutionPortConfiguration.Add(projectConfiguration);
                    }

                    SolutionPort solutionPort = new SolutionPort(solutionPath, solutionPortConfiguration);
                    var analysisRunResult = solutionPort.AnalysisRun();

                    foreach(var projectAction in analysisRunResult.Values)
                    {
                        Assert.IsTrue(projectAction.ToSummaryString()?.Length > 0);
                    }

                    StringBuilder str = new StringBuilder();
                    foreach (var k in analysisRunResult.Keys)
                    {
                        StringBuilder projectResults = new StringBuilder();
                        projectResults.AppendLine(k);
                        projectResults.AppendLine(analysisRunResult[k].ToString());
                        result.ProjectResults.Where(p => p.CsProjectPath == k).FirstOrDefault().ProjectAnalysisResult = projectResults.ToString();

                        str.Append(projectResults);
                    }

                    result.SolutionAnalysisResult = str.ToString();

                    var runResult = solutionPort.Run();
                    result.SolutionRunResult = runResult;

                    foreach (var project in result.ProjectResults)
                    {
                        project.CsProjectContent = File.ReadAllText(project.CsProjectPath);
                    }
                }
            }
            return result;
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
            foreach(var file in files)
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }

            var dirs = source.GetDirectories();
            foreach(var dir in dirs)
            {
                DirectoryInfo destinationSub = new DirectoryInfo(Path.Combine(target.FullName, dir.Name));
                CopyDirectory(dir, destinationSub);
            }
        }
    }
}

