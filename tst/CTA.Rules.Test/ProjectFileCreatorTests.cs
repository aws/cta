using System.Collections.Generic;
using System.IO;
using CTA.Rules.Models;
using CTA.Rules.ProjectFile;
using NUnit.Framework;

namespace CTA.Rules.Test
{
    public class ProjectFileCreatorTests
    {
        private ProjectFileCreator _mvcProjectFileCreator;
        private ProjectFileCreator _classLibraryProjectFileCreator;
        private ProjectFileCreator _webClassLibraryProjectFileCreator;

        private string _mvcProjectFilePath = "mvc.csproj";
        private string _classLibraryProjectFilePath = "classLibrary.csproj";
        private string _webClassLibraryProjectFilePath = "webClassLibrary.csproj";

        [SetUp]
        public void Setup()
        {
            CreateEmptyXmlDocument(_mvcProjectFilePath);
            CreateEmptyXmlDocument(_classLibraryProjectFilePath);
            CreateEmptyXmlDocument(_webClassLibraryProjectFilePath);

            var targetFramework = new List<string> { "netcoreapp3.1" };
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

            _mvcProjectFileCreator = new ProjectFileCreator(_mvcProjectFilePath, targetFramework, 
                packages, projectReferences, ProjectType.Mvc, metaRefs);
            _classLibraryProjectFileCreator = new ProjectFileCreator(_classLibraryProjectFilePath, targetFramework, 
                packages, projectReferences, ProjectType.ClassLibrary, metaRefs);
            _webClassLibraryProjectFileCreator = new ProjectFileCreator(_webClassLibraryProjectFilePath, targetFramework, 
                packages, projectReferences, ProjectType.WebClassLibrary, metaRefs);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_webClassLibraryProjectFilePath))
            {
                File.Delete(_webClassLibraryProjectFilePath);
            }
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
        
        private void CreateEmptyXmlDocument(string filePath)
        {
            File.WriteAllText(filePath, "<root></root>");
        }
    }
}