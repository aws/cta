using NUnit.Framework;
using System;
using System.IO;

namespace CTA.WebForms2Blazor.Tests.ProjectManagement
{
    [SetUpFixture]
    public class PartialProjectSetupFixture
    {
        public const string TestAreaDirectoryName = "TestingArea";
        public const string TestFilesDirectoryName = "TestFiles";
        public const string TestStructure1DirectoryName = "TestStructure1";
        public const string TestBlazorProjectDirectoryName = "BlazorTestProject";
        public const string TestClassFileName = "TestClassFile.cs";
        public const string TestXMLFileName = "TestXMLFile.xml";
        public const string NestedTestClassFileName = "NestedTestClassFile.cs";
        public const string NestedTestTextFileName = "NestedTestTextFile.txt";

        public static string WorkingDirectory = Environment.CurrentDirectory;
        public static string TestProjectPath => Directory.GetParent(WorkingDirectory).Parent.Parent.FullName;
        public static string TestingAreaPath => Path.Combine(TestProjectPath, TestAreaDirectoryName);
        public static string TestFilesPath => Path.Combine(TestingAreaPath, TestFilesDirectoryName);
        public static string TestStructure1Path => Path.Combine(TestFilesPath, TestStructure1DirectoryName);
        public static string TestBlazorProjectPath => Path.Combine(TestingAreaPath, TestBlazorProjectDirectoryName);
    }
}
