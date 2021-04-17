using System.IO;
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

        [SetUp]
        public void Setup()
        {
            // Setup an empty project folder for testing
            Directory.CreateDirectory(_testProjectDir);
            CreateEmptyXmlDocument(_testProjectPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testProjectDir))
            {
                Directory.Delete(_testProjectDir, true);
            }
        }

        private void CreateEmptyXmlDocument(string filePath)
        {
            File.WriteAllText(filePath, "<root></root>");
        }

        [Test]
        public void Folder_Update_for_Mvc_Project()
        {
            // Create MVC Content and Script foldel under MVC test project folderr
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Content));
            Directory.CreateDirectory(
                Path.Combine(_testProjectDir, Constants.Scripts));

            // Run FolderUpdate on the test project
            ProjectType projectType = ProjectType.Mvc;
            FolderUpdate folderUpdate =  new FolderUpdate(
                _testProjectPath, projectType);
            folderUpdate.Run();

            // Validate wwwroot foler gets created, Content and Script foler
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
        }
    }
}
