using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CTA.WebForms2Blazor.ProjectManagement;

namespace CTA.WebForms2Blazor.Tests.ProjectManagement
{
    public class ProjectBuilderTests
    {
        private ProjectBuilder _projectBuilder;

        [SetUp]
        public void SetUp()
        {
            _projectBuilder = new ProjectBuilder(PartialProjectSetupFixture.TestBlazorProjectPath);
        }

        [TearDown]
        public void TearDown()
        {
            ClearTestBlazorProjectDirectory();
        }

        [Test]
        [NonParallelizable]
        public void CreateRelativeDirectoryIfNotExists_Creates_Directory_In_Correct_Location()
        {
            var testDirectoryName = "folder1";
            var testTargetDirectory = Path.Combine(PartialProjectSetupFixture.TestBlazorProjectPath, testDirectoryName);

            Assert.False(Directory.Exists(testTargetDirectory));

            _projectBuilder.CreateRelativeDirectoryIfNotExists(testDirectoryName);

            Assert.True(Directory.Exists(testTargetDirectory));
        }

        [Test]
        [NonParallelizable]
        public void WriteFileBytesToProject_Writes_New_File_With_Full_Fidelity()
        {
            var testClassFilePath = Path.Combine(PartialProjectSetupFixture.TestFilesPath, PartialProjectSetupFixture.TestClassFileName);
            var testClassTargetPath = Path.Combine(PartialProjectSetupFixture.TestBlazorProjectPath, PartialProjectSetupFixture.TestClassFileName);
            byte[] originalBytesContent = null;
            byte[] newBytesContent = null;

            using (FileStream stream = File.OpenRead(testClassFilePath))
            {
                originalBytesContent = new byte[stream.Length];
                stream.Read(originalBytesContent, 0, originalBytesContent.Length);
            }

            Assert.False(File.Exists(testClassTargetPath));

            _projectBuilder.WriteFileBytesToProject(testClassTargetPath, originalBytesContent);

            Assert.True(File.Exists(testClassTargetPath));

            using (FileStream stream = File.OpenRead(testClassTargetPath))
            {
                newBytesContent = new byte[stream.Length];
                stream.Read(newBytesContent, 0, newBytesContent.Length);
            }

            Assert.True(originalBytesContent.SequenceEqual(newBytesContent));
        }

        [Test]
        [NonParallelizable]
        public void WriteFileBytesToProject_Creates_Missing_Parent_Directories()
        {
            var deepTestClassTargetParentPath = Path.Combine(PartialProjectSetupFixture.TestBlazorProjectPath, "folder1", "folder2");
            var deepTestClassTargetPath = Path.Combine(deepTestClassTargetParentPath, PartialProjectSetupFixture.TestClassFileName);
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            Assert.False(Directory.Exists(deepTestClassTargetParentPath));

            _projectBuilder.WriteFileBytesToProject(deepTestClassTargetPath, contentBytes);

            Assert.True(Directory.Exists(deepTestClassTargetParentPath));
            Assert.True(File.Exists(deepTestClassTargetPath));
        }

        [Test]
        [NonParallelizable]
        public void DeleteDirectoriesIfEmpty_Will_Not_Delete_Directory_With_Content()
        {
            var pathLimit = PartialProjectSetupFixture.TestingAreaPath;

            var basePath = Path.Combine(pathLimit, "Dir1");
            var filePath = Path.Combine(basePath, "testfile.txt");
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            _projectBuilder.WriteFileBytesToProject(filePath, contentBytes);
            _projectBuilder.DeleteDirectoriesIfEmpty(basePath, pathLimit);

            Assert.True(Directory.Exists(basePath));
            Assert.True(File.Exists(filePath));

            Directory.Delete(basePath, true);
        }

        [Test]
        [NonParallelizable]
        public void DeleteDirectoriesIfEmpty_Will_Not_Delete_Directory_With_Content_One_Level_Up()
        {
            var pathLimit = PartialProjectSetupFixture.TestingAreaPath;

            var contentPath = Path.Combine(pathLimit, "Dir1");
            var emptyPath = Path.Combine(contentPath, "Dir2");
            var filePath = Path.Combine(contentPath, "testfile.txt");
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            _projectBuilder.WriteFileBytesToProject(filePath, contentBytes);
            _projectBuilder.DeleteDirectoriesIfEmpty(emptyPath, pathLimit);

            Assert.False(Directory.Exists(emptyPath));
            Assert.True(Directory.Exists(contentPath));
            Assert.True(File.Exists(filePath));

            Directory.Delete(contentPath, true);
        }

        [Test]
        [NonParallelizable]
        public void DeleteDirectoriesIfEmpty_Will_Not_Delete_Directory_With_Content_Multiple_Levels_Up()
        {
            var pathLimit = PartialProjectSetupFixture.TestingAreaPath;

            var contentPath = Path.Combine(pathLimit, "Dir1");
            var emptyPath = Path.Combine(contentPath, "Dir2");
            var filePath = Path.Combine(contentPath, "Dir3", "Dir4", "testfile.txt");
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            _projectBuilder.WriteFileBytesToProject(filePath, contentBytes);
            _projectBuilder.DeleteDirectoriesIfEmpty(emptyPath, pathLimit);

            Assert.False(Directory.Exists(emptyPath));
            Assert.True(Directory.Exists(contentPath));
            Assert.True(File.Exists(filePath));

            Directory.Delete(contentPath, true);
        }

        [Test]
        [NonParallelizable]
        public void DeleteDirectoriesIfEmpty_Will_Not_Delete_Past_PathLimit_Directory()
        {
            var pathLimit = Path.Combine(PartialProjectSetupFixture.TestingAreaPath, "Dir1");

            var basePath = Path.Combine(pathLimit, "Dir2");
            Directory.CreateDirectory(basePath);

            _projectBuilder.DeleteDirectoriesIfEmpty(basePath, pathLimit);

            Assert.True(Directory.Exists(pathLimit));
            Assert.False(Directory.Exists(basePath));

            Directory.Delete(pathLimit, true);
        }

        [Test]
        [NonParallelizable]
        public void DeleteFileAndEmptyDirectories_Will_Only_Delete_File_If_Sibling_Content_Exists()
        {
            var pathLimit = PartialProjectSetupFixture.TestingAreaPath;

            var basePath = Path.Combine(pathLimit, "Dir1");
            var filePath = Path.Combine(basePath, "testfile.txt");
            var siblingFilePath = Path.Combine(basePath, "testfile2.txt");
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            _projectBuilder.WriteFileBytesToProject(filePath, contentBytes);
            _projectBuilder.WriteFileBytesToProject(siblingFilePath, contentBytes);
            _projectBuilder.DeleteFileAndEmptyDirectories(filePath, pathLimit);

            Assert.False(File.Exists(filePath));
            Assert.True(Directory.Exists(basePath));
            Assert.True(File.Exists(siblingFilePath));

            Directory.Delete(basePath, true);
        }

        [Test]
        [NonParallelizable]
        public void DeleteFileAndEmptyDirectories_Will_Delete_Parent_Directory_If_No_Sibling_Content_Exists()
        {
            var pathLimit = PartialProjectSetupFixture.TestingAreaPath;

            var basePath = Path.Combine(pathLimit, "Dir1");
            var filePath = Path.Combine(basePath, "testfile.txt");
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            _projectBuilder.WriteFileBytesToProject(filePath, contentBytes);
            _projectBuilder.DeleteFileAndEmptyDirectories(filePath, pathLimit);

            Assert.False(File.Exists(filePath));
            Assert.False(Directory.Exists(basePath));
            Assert.True(Directory.Exists(pathLimit));
        }

        public void ClearTestBlazorProjectDirectory()
        {
            if (Directory.Exists(PartialProjectSetupFixture.TestBlazorProjectPath))
            {
                Directory.Delete(PartialProjectSetupFixture.TestBlazorProjectPath, true);
            }
        }
    }
}
