using System.Collections.Generic;
using System.IO;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.ProjectFile;
using NUnit.Framework;
using Shouldly;

namespace CTA.Rules.Test
{
    public class ProjectFileCreatorTests
    {
        private ProjectFileCreator _vanillaProjectFileCreator;
        private ProjectFileCreator _mvcProjectFileCreator;
        private ProjectFileCreator _classLibraryProjectFileCreator;
        private ProjectFileCreator _webClassLibraryProjectFileCreator;

        private readonly string _vanillaMvcProfileFilePath = "vanilla-mvc.csproj";
        private readonly string _mvcProjectFilePath = "mvc.csproj";
        private readonly string _classLibraryProjectFilePath = "classLibrary.csproj";
        private readonly string _webClassLibraryProjectFilePath = "webClassLibrary.csproj";

        [SetUp]
        public void Setup()
        {
            CreateVanillaXmlProject(_vanillaMvcProfileFilePath);
            CreateEmptyXmlDocument(_mvcProjectFilePath);
            CreateEmptyXmlDocument(_classLibraryProjectFilePath);
            CreateEmptyXmlDocument(_webClassLibraryProjectFilePath);

            var targetFramework = new List<string> { Constants.DefaultCoreVersion };
            var sourceFramework = new List<string> { Constants.DefaultNetFrameworkVersion };
            var packages = new Dictionary<string, string>
            {
                { "MyFirstPackage", "1.0.0" },
                { "MySecondPackage", "2.0.0" }
            };
            var projectReferences = new List<string>
            {
                "TheMainProject",
                "TheDependency"
            };
            var metaRefs = new List<string>
            {
                @"C:\\RandomFile.dll"
            };

            _vanillaProjectFileCreator =
                new ProjectFileCreator(
                    _vanillaMvcProfileFilePath,
                    targetFramework,
                    packages: packages,
                    projectReferences: new List<string>(),
                    ProjectType.CoreMvc,
                    metaReferences: new List<string>());

            _mvcProjectFileCreator = 
                new ProjectFileCreator(
                    _mvcProjectFilePath, 
                    targetFramework, 
                    packages,
                    projectReferences, 
                    ProjectType.Mvc,
                    metaRefs, 
                    sourceFramework);

            _classLibraryProjectFileCreator = 
                new ProjectFileCreator(
                    _classLibraryProjectFilePath, 
                    targetFramework, 
                    packages, 
                    projectReferences, 
                    ProjectType.ClassLibrary, 
                    metaRefs, 
                    sourceFramework);

            _webClassLibraryProjectFileCreator = 
                new ProjectFileCreator(
                    _webClassLibraryProjectFilePath, 
                    targetFramework, 
                    packages,
                    projectReferences, 
                    ProjectType.WebClassLibrary, 
                    metaRefs, 
                    sourceFramework);
        }

        [TearDown]
        public void TearDown()
        {
            var testFiles = new[]
            {
                _classLibraryProjectFilePath,
                _webClassLibraryProjectFilePath,
                _mvcProjectFilePath,
                _vanillaMvcProfileFilePath

            };

            foreach (var testFile in testFiles)
                if (File.Exists(testFile))
                    File.Delete(testFile);
        }

        [Test]
        public void CanAddProjectReferencesToVanillaMvcProject()
        {
            bool success;

            success = _vanillaProjectFileCreator.Create();

            success.ShouldBeTrue();
        }

        [Test]
        public void Create_Produces_Formatted_Mvc_Project_File()
        {
            _mvcProjectFileCreator.Create();
            var projectFileContents = File.ReadAllText(_mvcProjectFilePath);

            var expectedProjectFileContents =
@"<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MyFirstPackage"" Version=""1.0.0"" />
    <PackageReference Include=""MySecondPackage"" Version=""2.0.0"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""TheMainProject"" />
    <ProjectReference Include=""TheDependency"" />
  </ItemGroup>
  <ItemGroup Label=""PortingInfo"">
  <!-- DO NOT REMOVE WHILE PORTING
  C:\\RandomFile.dll
  -->
  </ItemGroup>
</Project>";

            Assert.AreEqual(expectedProjectFileContents, projectFileContents);
        }

        [Test]
        public void Create_Produces_Formatted_ClassLibrary_Project_File()
        {
            _classLibraryProjectFileCreator.Create();
            var projectFileContents = File.ReadAllText(_classLibraryProjectFilePath);

            var expectedProjectFileContents =
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MyFirstPackage"" Version=""1.0.0"" />
    <PackageReference Include=""MySecondPackage"" Version=""2.0.0"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""TheMainProject"" />
    <ProjectReference Include=""TheDependency"" />
  </ItemGroup>
  <ItemGroup Label=""PortingInfo"">
  <!-- DO NOT REMOVE WHILE PORTING
  C:\\RandomFile.dll
  -->
  </ItemGroup>
</Project>";

            Assert.AreEqual(expectedProjectFileContents, projectFileContents);
        }

        [Test]
        public void Create_Produces_Formatted_WebClassLibrary_Project_File()
        {
            _webClassLibraryProjectFileCreator.Create();
            var projectFileContents = File.ReadAllText(_webClassLibraryProjectFilePath);

            var expectedProjectFileContents =
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include=""Microsoft.AspNetCore.App"" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""MyFirstPackage"" Version=""1.0.0"" />
    <PackageReference Include=""MySecondPackage"" Version=""2.0.0"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""TheMainProject"" />
    <ProjectReference Include=""TheDependency"" />
  </ItemGroup>
  <ItemGroup Label=""PortingInfo"">
  <!-- DO NOT REMOVE WHILE PORTING
  C:\\RandomFile.dll
  -->
  </ItemGroup>
</Project>";

            Assert.AreEqual(expectedProjectFileContents, projectFileContents);
        }

        private void CreateVanillaXmlProject(string vanillaMvcProfileFilePath)
        {
            var vanillaMvcProject =
@"<Project Sdk = ""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
</Project>";

            File.WriteAllText(_vanillaMvcProfileFilePath, vanillaMvcProject);
        }

        private void CreateEmptyXmlDocument(string filePath)
        {
            File.WriteAllText(filePath, "<root></root>");
        }
    }
}