using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests
{
    // TODO: Make a more complex test fixture setup
    // once more complex functionality is implemented
    [TestFixture]
    public class BlazorProjectBuilderTests
    {
        private const string TEST_AREA_DIRECTORY_NAME = "TestingArea";
        private const string TEST_FILES_DIRECTORY_NAME = "TestFiles";
        private const string TEST_BLAZOR_PROJECT_DIRECTORY_NAME = "BlazorTestProject";
        private const string TEST_CLASS_FILE_NAME = "TestClassFile.cs";

        private string _testProjectPath;
        private string _testingAreaPath;
        private string _testFilesPath;
        private string _testBlazorProjectPath;
        private BlazorProjectBuilder _projectBuilder;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _testProjectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            _testingAreaPath = Path.Combine(_testProjectPath, TEST_AREA_DIRECTORY_NAME);
            _testFilesPath = Path.Combine(_testingAreaPath, TEST_FILES_DIRECTORY_NAME);
            _testBlazorProjectPath = Path.Combine(_testingAreaPath, TEST_BLAZOR_PROJECT_DIRECTORY_NAME);

            ClearTestBlazorProjectDirectory();
        }

        [SetUp]
        public void Setup()
        {
            _projectBuilder = new BlazorProjectBuilder(_testBlazorProjectPath);
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
            var testTargetDirectory = Path.Combine(_testBlazorProjectPath, testDirectoryName);

            Assert.False(Directory.Exists(testTargetDirectory));

            _projectBuilder.CreateRelativeDirectoryIfNotExists(testDirectoryName);

            Assert.True(Directory.Exists(testTargetDirectory));
        }

        [Test]
        [NonParallelizable]
        public void WriteFileBytesToProject_Writes_New_File_With_Full_Fidelity()
        {
            var testClassFilePath = Path.Combine(_testFilesPath, TEST_CLASS_FILE_NAME);
            var testClassTargetPath = Path.Combine(_testBlazorProjectPath, TEST_CLASS_FILE_NAME);
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
            var deepTestClassTargetParentPath = Path.Combine(_testBlazorProjectPath, "folder1", "folder2");
            var deepTestClassTargetPath = Path.Combine(deepTestClassTargetParentPath, TEST_CLASS_FILE_NAME);
            var contentBytes = Encoding.UTF8.GetBytes("namespace SomeNS { public class NewClass {} }");

            Assert.False(Directory.Exists(deepTestClassTargetParentPath));

            _projectBuilder.WriteFileBytesToProject(deepTestClassTargetPath, contentBytes);

            Assert.True(Directory.Exists(deepTestClassTargetParentPath));
            Assert.True(File.Exists(deepTestClassTargetPath));
        }

        public void ClearTestBlazorProjectDirectory()
        {
            if (Directory.Exists(_testBlazorProjectPath))
            {
                Directory.Delete(_testBlazorProjectPath, true);
            }
        }
    }
}
