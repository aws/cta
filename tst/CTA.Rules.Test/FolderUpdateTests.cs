using System.IO;
using System.IO.Compression;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using CTA.Rules.Config;
using NUnit.Framework;

namespace CTA.Rules.Test
{
    public class FolderUpdateTests
    {
        private string _testProjectDir = Path.Combine(
            Directory.GetCurrentDirectory(), "TestProject");
        private string _testProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(), "TestProject", "TestProject.csproj");
        private string _vbTestProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(), "TestProject", "TestProject.vbproj");

        [SetUp]
        public void Setup()
        {
            // Setup an empty project folder for testing
            Directory.CreateDirectory(_testProjectDir);
            CreateEmptyXmlDocument(_testProjectPath);

            //Download resources from S3
            Utils.DownloadFilesToFolder(Constants.S3TemplatesBucketUrl, Constants.ResourcesExtractedPath, Constants.TemplateFiles);
        }

        [TearDown]
        public void TearDown()
        {
            // Delete resources
            Directory.Delete(Constants.ResourcesExtractedPath, true);
            // Delete test project
            if (Directory.Exists(_testProjectDir))
            {
                Directory.Delete(_testProjectDir, true);
            }
        }

        private void CreateEmptyXmlDocument(string filePath)
        {
            File.WriteAllText(filePath, "<root></root>");
        }

        private void Cleanup(string dirPath)
        {
            // This method removes all files and subfolders within dirPath and retains the root folder
            DirectoryInfo directory = new DirectoryInfo(dirPath);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        [Test]
        public void Folder_Update_for_Mvc_Project()
        {
            // Create MVC Content and Script foldel under MVC test project folder
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Content));
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Scripts));

            // Run FolderUpdate on the test project
            ProjectType projectType = ProjectType.Mvc;
            FolderUpdate folderUpdate =  new FolderUpdate(
                _testProjectPath, projectType);
            folderUpdate.Run();

            // Validate wwwroot folder gets created, and Content and Script folders
            // get moved under wwwroot folder
            Assert.False(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Content))); 
            Assert.False(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Scripts)));
            Assert.True(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Wwwroot)));
            Assert.True(Directory.Exists(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Content)));
            Assert.True(Directory.Exists(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Scripts)));

            // Validate Program.cs and Startup.cs files are created
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Program.ToString() + ".cs")));
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Startup.ToString() + ".cs")));

            Cleanup(_testProjectDir);
        }

        [Test]
        public void Folder_Update_for_Mvc_Project_When_Wwwroot_Folder_Exists_In_Wwwroot()
        {
            // Create MVC Content and Script folders under MVC test project folder
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Content));
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Scripts));

            // Create target wwwroot folder
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Wwwroot));

            // Run FolderUpdate on the test project
            ProjectType projectType = ProjectType.Mvc;
            FolderUpdate folderUpdate =  new FolderUpdate(
                _testProjectPath, projectType);
            folderUpdate.Run();

            // Validate Content and Script folders get moved under wwwroot folder
            Assert.False(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Content)));
            Assert.False(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Scripts)));
            Assert.True(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Wwwroot)));
            Assert.True(Directory.Exists(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Content)));
            Assert.True(Directory.Exists(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Scripts)));

            // Validate Program.cs and Startup.cs files are created
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Program.ToString() + ".cs")));
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Startup.ToString() + ".cs")));

            Cleanup(_testProjectDir);
        }

        [Test]
        public void Folder_Update_for_Mvc_Project_When_Content_Scripts_Folders_Already_Exist_In_Wwwroot()
        {
            // Create MVC Content and Script folders under MVC test project folder
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Content));
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Scripts));

            // Create target wwwroot folder
            // And create Content and Scripts folders under wwwroot
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Wwwroot));
            Directory.CreateDirectory(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Content));
            Directory.CreateDirectory(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Scripts));


            // Run FolderUpdate on the test project
            ProjectType projectType = ProjectType.Mvc;
            FolderUpdate folderUpdate =  new FolderUpdate(
                _testProjectPath, projectType);
            folderUpdate.Run();

            // Validate Content and Script folders are not moved under wwwroot since they already exist in wwwroot
            Assert.True(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Content)));
            Assert.True(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Scripts)));
            Assert.True(Directory.Exists(
                Path.Combine(_testProjectDir, Constants.Wwwroot)));
            Assert.True(Directory.Exists(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Content)));
            Assert.True(Directory.Exists(Path.Combine(
                _testProjectDir, Constants.Wwwroot, Constants.Scripts)));

            // Validate Program.cs and Startup.cs files are created
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Program.ToString() + ".cs")));
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Startup.ToString() + ".cs")));

            Cleanup(_testProjectDir);
        }

        [Test]
        public void Folder_Update_for_WebApi_Project()
        {
            // Run FolderUpdate on the test project
            ProjectType projectType = ProjectType.WebApi;
            FolderUpdate folderUpdate =  new FolderUpdate(
                _testProjectPath, projectType);
            folderUpdate.Run();

            // Validate Program.cs and Startup.cs files are created
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Program.ToString() + ".cs")));
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Startup.ToString() + ".cs")));

            Cleanup(_testProjectDir);
        }

        [Test]
        public void Folder_Update_for_WebApi_Project_Vb()
        {
            // Run FolderUpdate on the test project
            ProjectType projectType = ProjectType.WebApi;
            FolderUpdate folderUpdate = new FolderUpdate(
                _vbTestProjectPath, projectType);
            folderUpdate.Run();

            // Validate Program.cs and Startup.cs files are created
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Program.ToString() + FileExtension.VisualBasic)));
            Assert.True(File.Exists(Path.Combine(
                _testProjectDir, FileTypeCreation.Startup.ToString() + FileExtension.VisualBasic)));

            Cleanup(_testProjectDir);
        }
    }
}
