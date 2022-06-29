using System;
using System.IO;
using CTA.Rules.Config;
using CTA.WebForms.Services;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CTA.WebForms.Tests.FileConverters
{
    [SetUpFixture]
    public class FileConverterSetupFixture : WebFormsTestBase
    {
        //These are full paths
        public static string TestProjectPath;
        public static string TestFilesDirectoryPath;

        public static string TestCodeFilePath;
        public static string TestWebConfigFilePath;
        public static string TestStaticResourceFilePath;
        public static string TestStaticFilePath;
        public static string TestViewFilePath;
        public static string TestHyperLinkControlFilePath;
        public static string TestButtonControlFilePath;
        public static string TestLabelControlFilePath;
        public static string TestListViewControlFilePath;
        public static string TestGridViewControlFilePath;
        public static string TestContentPlaceHolderControlFilePath;
        public static string TestContentControlFilePath;
        public static string TestSiteMasterFilePath;
        public static string TestDirectiveFilePath;
        public static string TestAreaFullPath;

        private WorkspaceManagerService _blazorWorkspaceManager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            TestProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            TestFilesDirectoryPath = Path.Combine(TestProjectPath, Path.Combine("TestingArea", "TestFiles"));
            TestCodeFilePath = Path.Combine(TestFilesDirectoryPath, "TestClassFile.cs");
            TestWebConfigFilePath = Path.Combine(TestFilesDirectoryPath, "web.config");
            TestStaticFilePath = Path.Combine(TestFilesDirectoryPath, "SampleStaticFile.csv");
            TestStaticResourceFilePath = Path.Combine(TestFilesDirectoryPath, "SampleStaticResourceFile.png");
            TestViewFilePath = Path.Combine(TestFilesDirectoryPath, "SampleViewFile.aspx");
            TestHyperLinkControlFilePath = Path.Combine(TestFilesDirectoryPath, "HyperLinkControlOnly.aspx");
            TestButtonControlFilePath = Path.Combine(TestFilesDirectoryPath, "ButtonControlOnly.aspx");
            TestLabelControlFilePath = Path.Combine(TestFilesDirectoryPath, "LabelControlOnly.aspx");
            TestListViewControlFilePath = Path.Combine(TestFilesDirectoryPath, "ListViewControlOnly.aspx");
            TestGridViewControlFilePath = Path.Combine(TestFilesDirectoryPath, "GridViewControlOnly.aspx");
            TestContentPlaceHolderControlFilePath =
                Path.Combine(TestFilesDirectoryPath, "ContentPlaceHolderControlOnly.aspx");
            TestContentControlFilePath = Path.Combine(TestFilesDirectoryPath, "ContentControlOnly.aspx");
            TestSiteMasterFilePath = Path.Combine(TestFilesDirectoryPath, "SampleSiteMasterFile.Master");
            TestDirectiveFilePath = Path.Combine(TestFilesDirectoryPath, "DirectiveOnly.aspx");
            TestAreaFullPath = Path.Combine(TestProjectPath, TestFilesDirectoryPath);
            
            _blazorWorkspaceManager = new WorkspaceManagerService();
            Utils.DownloadFilesToFolder(Constants.S3TemplatesBucketUrl, Constants.ResourcesExtractedPath, Constants.TemplateFiles);
        }
        
    }
}