using NUnit.Framework;
using System;
using System.IO;

namespace CTA.WebForms2Blazor.Tests.ProjectManagement
{
    [SetUpFixture]
    public class PartialProjectSetupFixture
    {
        public const string TEST_AREA_DIRECTORY_NAME = "TestingArea";
        public const string TEST_FILES_DIRECTORY_NAME = "TestFiles";
        public const string TEST_STRUCTURE_1_DIRECTORY_NAME = "TestStructure1";
        public const string TEST_BLAZOR_PROJECT_DIRECTORY_NAME = "BlazorTestProject";
        public const string TEST_CLASS_FILE_NAME = "TestClassFile.cs";
        public const string TEST_XML_FILE_NAME = "TestXMLFile.xml";
        public const string NESTED_TEST_CLASS_FILE_NAME = "NestedTestClassFile.cs";
        public const string NESTED_TEST_TEXT_FILE_NAME = "NestedTestTextFile.txt";

        public static string WorkingDirectory = Environment.CurrentDirectory;
        public static string TestProjectPath => Directory.GetParent(WorkingDirectory).Parent.Parent.FullName;
        public static string TestingAreaPath => Path.Combine(TestProjectPath, TEST_AREA_DIRECTORY_NAME);
        public static string TestFilesPath => Path.Combine(TestingAreaPath, TEST_FILES_DIRECTORY_NAME);
        public static string TestStructure1Path => Path.Combine(TestFilesPath, TEST_STRUCTURE_1_DIRECTORY_NAME);
        public static string TestBlazorProjectPath => Path.Combine(TestingAreaPath, TEST_BLAZOR_PROJECT_DIRECTORY_NAME);
    }
}
