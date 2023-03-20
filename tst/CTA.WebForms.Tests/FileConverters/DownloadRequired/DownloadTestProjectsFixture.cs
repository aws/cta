using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using CTA.Rules.Config;
using CTA.Rules.Test;
using NUnit.Framework;

namespace CTA.WebForms.Tests.FileConverters.DownloadRequired
{
    public class DownloadTestProjectsFixture : AwsRulesBaseTest
    {
        private string _tempDir;
        private string _testRunFolder;
        private string _downloadLocation;
        private static string _eShopOnBlazorSolutionFilePath;
        private static string _eShopOnBlazorSolutionPath;
        private static string _eShopLegacyWebFormsProjectPath;
        
        public static string EShopOnBlazorSolutionPath { get { return _eShopOnBlazorSolutionPath; } }
        public static string EShopOnBlazorSolutionFilePath { get { return _eShopOnBlazorSolutionFilePath; } }
        public static string EShopLegacyWebFormsProjectPath { get { return _eShopLegacyWebFormsProjectPath; } }

        [OneTimeSetUp]
        public void Setup()
        {
            Setup(GetType());
            _tempDir = GetTstPath(Path.Combine(new[] {"Projects", "Temp", "W2B"}));
            DownloadTestProjects();

            _eShopOnBlazorSolutionFilePath = CopySolutionDirToUniqueTempDir("eShopOnBlazor.sln", _tempDir);
            _eShopOnBlazorSolutionPath = Directory.GetParent(EShopOnBlazorSolutionFilePath).FullName;
            _testRunFolder = EShopOnBlazorSolutionPath;

            _eShopLegacyWebFormsProjectPath = Utils.GetProjectPaths(EShopOnBlazorSolutionPath)
                .First(filePath =>
                    filePath.EndsWith("eShopLegacyWebForms.csproj", StringComparison.InvariantCultureIgnoreCase));

        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            DeleteDir(0);
        }

        private void DownloadTestProjects()
        {
            var tempDirectory = Directory.CreateDirectory(_tempDir);
            _downloadLocation = Path.Combine(tempDirectory.FullName, "w2b");

            var fileName = Path.Combine(tempDirectory.FullName, @"TestProjects.zip");
            Utils.SaveFileFromGitHub(fileName, GithubInfo.TestGithubOwner, GithubInfo.TestGithubRepo,
                GithubInfo.TestGithubTag);
            ZipFile.ExtractToDirectory(fileName, _downloadLocation, true);
        }
        
        private void DeleteDir(int retries)
        {
            if (retries <= 10)
            {
                try
                {
                    Directory.Delete(_tempDir, true);
                    Directory.Delete(_testRunFolder, true);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    DeleteDir(retries + 1);
                }
            }
        }
    }
}
