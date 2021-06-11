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

        public void ClearTestBlazorProjectDirectory()
        {
            if (Directory.Exists(PartialProjectSetupFixture.TestBlazorProjectPath))
            {
                Directory.Delete(PartialProjectSetupFixture.TestBlazorProjectPath, true);
            }
        }
    }
}
