using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using CTA.Rules.Config;
using CTA.Rules.Test;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests
{
    [SetUpFixture]
    public class DownloadTestProjectsFixture : AwsRulesBaseTest
    {
        public string TempDir;
        public string CopyFolder;
        public string DownloadLocation;
        public string EShopOnBlazorSolutionPath;
        public string EShopOnBlazorSolutionFilePath;
        public string EShopLegacyWebFormsProjectPath;

        [OneTimeSetUp]
        public void Setup()
        {
            Setup(GetType());
            TempDir = GetTstPath(Path.Combine(new[] {"Projects", "Temp", "W2B"}));
            DownloadTestProjects();

            EShopOnBlazorSolutionFilePath = CopySolutionFolderToTemp("eShopOnBlazor.sln", TempDir);
            EShopOnBlazorSolutionPath = Directory.GetParent(EShopOnBlazorSolutionFilePath).FullName;

            CopyFolder = Directory.GetParent(EShopOnBlazorSolutionPath).FullName;

            EShopLegacyWebFormsProjectPath = Directory
                .EnumerateFiles(EShopOnBlazorSolutionPath, "*.csproj", SearchOption.AllDirectories)
                .First(filePath =>
                    filePath.EndsWith("eShopLegacyWebForms.csproj", StringComparison.InvariantCultureIgnoreCase));

        }

        private void DownloadTestProjects()
        {
            var tempDirectory = Directory.CreateDirectory(TempDir);
            DownloadLocation = Path.Combine(tempDirectory.FullName, "d");

            var fileName = Path.Combine(tempDirectory.Parent.FullName, @"TestProjects.zip");
            Utils.SaveFileFromGitHub(fileName, GithubInfo.TestGithubOwner, GithubInfo.TestGithubRepo,
                GithubInfo.TestGithubTag);
            ZipFile.ExtractToDirectory(fileName, DownloadLocation, true);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            DeleteDir(0);
        }

        private void DeleteDir(int retries)
        {
            if (retries <= 10)
            {
                try
                {
                    Directory.Delete(TempDir, true);
                    Directory.Delete(CopyFolder, true);
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
